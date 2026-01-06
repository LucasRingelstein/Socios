using System.Text.Json;
using MercadoPago.Client.Preapproval;
using MercadoPago.Config;
using MercadoPago.Resource.Preapproval;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Socios.Data;
using Socios.Dtos;
using Socios.Models;
using Socios.Options;

namespace Socios.Services;

public class SubscriptionService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly MercadoPagoSettings _settings;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(ApplicationDbContext dbContext, IOptions<MercadoPagoSettings> settings, ILogger<SubscriptionService> logger)
    {
        _dbContext = dbContext;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<SubscriptionResponse> CreateAsync(CreateSubscriptionRequest request)
    {
        EnsureCredentials();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.PayerEmail);
        if (user is null)
        {
            user = new User
            {
                Email = request.PayerEmail,
                FullName = request.UserFullName
            };
            _dbContext.Users.Add(user);
        }

        var planName = ResolvePlanName(request);
        var frequencyType = NormalizeFrequencyType(request.Interval ?? request.FrequencyType);

        var subscription = new Subscription
        {
            PlanName = planName,
            TransactionAmount = request.TransactionAmount,
            CurrencyId = string.IsNullOrWhiteSpace(request.CurrencyId) ? "USD" : request.CurrencyId,
            Frequency = request.Frequency <= 0 ? 1 : request.Frequency,
            FrequencyType = frequencyType,
            User = user,
            Status = "pending",
            BackUrl = request.BackUrl ?? _settings.BackUrl,
            CallbackUrl = request.CallbackUrl ?? _settings.WebhookCallbackUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Subscriptions.Add(subscription);
        await _dbContext.SaveChangesAsync();

        var preapprovalClient = new PreapprovalClient();
        var preapprovalRequest = new PreapprovalCreateRequest
        {
            Reason = planName,
            ExternalReference = subscription.Id.ToString(),
            AutoRecurring = new PreapprovalAutoRecurringRequest
            {
                CurrencyId = subscription.CurrencyId,
                TransactionAmount = request.TransactionAmount,
                Frequency = subscription.Frequency,
                FrequencyType = subscription.FrequencyType
            },
            BackUrl = subscription.BackUrl,
            PayerEmail = request.PayerEmail
        };

        Preapproval? mpResponse = null;
        try
        {
            mpResponse = await preapprovalClient.CreateAsync(preapprovalRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Mercado Pago preapproval for subscription {SubscriptionId}", subscription.Id);
            throw;
        }

        if (mpResponse != null)
        {
            subscription.MercadoPagoPreapprovalId = mpResponse.Id;
            subscription.Status = mpResponse.Status ?? subscription.Status;
            subscription.UpdatedAt = DateTime.UtcNow;

            _dbContext.PaymentStatusLogs.Add(new PaymentStatusLog
            {
                SubscriptionId = subscription.Id,
                MercadoPagoEventId = mpResponse.Id,
                Status = mpResponse.Status,
                Topic = "subscription_create",
                RawPayload = JsonSerializer.Serialize(mpResponse)
            });

            await _dbContext.SaveChangesAsync();
        }

        return MapSubscriptionResponse(subscription, user, mpResponse);
    }

    public async Task<SubscriptionResponse?> GetAsync(Guid id, bool refreshFromMercadoPago = true)
    {
        var subscription = await _dbContext.Subscriptions.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
        if (subscription is null)
        {
            return null;
        }

        if (refreshFromMercadoPago && !string.IsNullOrWhiteSpace(subscription.MercadoPagoPreapprovalId))
        {
            try
            {
                EnsureCredentials();
                var client = new PreapprovalClient();
                var mpSubscription = await client.GetAsync(subscription.MercadoPagoPreapprovalId);
                if (mpSubscription != null && !string.IsNullOrWhiteSpace(mpSubscription.Status) &&
                    !string.Equals(subscription.Status, mpSubscription.Status, StringComparison.OrdinalIgnoreCase))
                {
                    subscription.Status = mpSubscription.Status!;
                    subscription.UpdatedAt = DateTime.UtcNow;
                    _dbContext.PaymentStatusLogs.Add(new PaymentStatusLog
                    {
                        SubscriptionId = subscription.Id,
                        MercadoPagoEventId = mpSubscription.Id,
                        Status = mpSubscription.Status,
                        Topic = "subscription_status_refresh",
                        RawPayload = JsonSerializer.Serialize(mpSubscription)
                    });
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to refresh subscription {SubscriptionId} from Mercado Pago", subscription.Id);
            }
        }

        return MapSubscriptionResponse(subscription, subscription.User);
    }

    public async Task ProcessWebhookAsync(string rawBody, WebhookEvent? webhookEvent)
    {
        var preapprovalId = webhookEvent?.Data?.Id ?? webhookEvent?.Id;
        if (string.IsNullOrWhiteSpace(preapprovalId))
        {
            _logger.LogWarning("Mercado Pago webhook received without preapproval id. Payload: {Payload}", rawBody);
            return;
        }

        Subscription? subscription = await _dbContext.Subscriptions.FirstOrDefaultAsync(s =>
            s.MercadoPagoPreapprovalId == preapprovalId || s.Id.ToString() == preapprovalId);

        Preapproval? mpSubscription = null;

        try
        {
            EnsureCredentials();
            var client = new PreapprovalClient();
            mpSubscription = await client.GetAsync(preapprovalId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch Mercado Pago subscription for id {PreapprovalId}", preapprovalId);
        }

        if (subscription != null && mpSubscription != null)
        {
            subscription.Status = mpSubscription.Status ?? subscription.Status;
            subscription.UpdatedAt = DateTime.UtcNow;
        }

        _dbContext.PaymentStatusLogs.Add(new PaymentStatusLog
        {
            SubscriptionId = subscription?.Id ?? Guid.Empty,
            MercadoPagoEventId = preapprovalId,
            Status = mpSubscription?.Status ?? webhookEvent?.Action,
            Topic = webhookEvent?.Type,
            RawPayload = rawBody
        });

        await _dbContext.SaveChangesAsync();
    }

    private static SubscriptionResponse MapSubscriptionResponse(Subscription subscription, User? user, Preapproval? preapproval = null)
    {
        return new SubscriptionResponse
        {
            Id = subscription.Id,
            PlanName = subscription.PlanName,
            Status = subscription.Status,
            MercadoPagoPreapprovalId = subscription.MercadoPagoPreapprovalId,
            InitPoint = preapproval?.InitPoint,
            PayerEmail = user?.Email,
            TransactionAmount = subscription.TransactionAmount,
            CurrencyId = subscription.CurrencyId,
            Frequency = subscription.Frequency,
            FrequencyType = subscription.FrequencyType,
            CreatedAt = subscription.CreatedAt,
            BackUrl = subscription.BackUrl,
            CallbackUrl = subscription.CallbackUrl
        };
    }

    private void EnsureCredentials()
    {
        if (string.IsNullOrWhiteSpace(_settings.AccessToken))
        {
            throw new InvalidOperationException("Mercado Pago access token must be configured via configuration or environment variables.");
        }

        MercadoPagoConfig.AccessToken = _settings.AccessToken;
    }

    private static string ResolvePlanName(CreateSubscriptionRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.PlanName))
        {
            return request.PlanName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            return request.Description.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Plan))
        {
            return request.Plan.Trim();
        }

        return "Subscription";
    }

    private static string NormalizeFrequencyType(string? interval)
    {
        if (string.IsNullOrWhiteSpace(interval))
        {
            return "months";
        }

        return interval.ToLowerInvariant() switch
        {
            "monthly" or "month" or "months" => "months",
            "weekly" or "week" or "weeks" => "weeks",
            "daily" or "day" or "days" => "days",
            "yearly" or "year" or "years" => "months",
            _ => "months"
        };
    }
}
