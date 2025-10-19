using CodePunk.Conveyancing.Api.Features.Health;
using CodePunk.Conveyancing.Api.Features.Conveyances.Create;
using CodePunk.Conveyancing.Api.Features.Conveyances.Get;
using CodePunk.Conveyancing.Api.Agents;
using CodePunk.Conveyancing.Api.Features.Drafts.CreateTitleEnquiryDraft;
using CodePunk.Conveyancing.Api.Features.Drafts.GetDraft;
using CodePunk.Conveyancing.Api.Features.Drafts.ListDrafts;
using CodePunk.Conveyancing.Api.Features.Drafts.ApproveDraft;
using CodePunk.Conveyancing.Api.Features.Drafts.RejectDraft;
using CodePunk.Conveyancing.Api.Data;
using Microsoft.EntityFrameworkCore;
using CodePunk.Conveyancing.Api.Features.Outbox.List;
using CodePunk.Conveyancing.Api.Features.Outbox.Get;
using CodePunk.Conveyancing.Api.Features.Outbox.Create;
using CodePunk.Conveyancing.Api.Features.Outbox.Mutate;
using CodePunk.Conveyancing.Api.Infrastructure.Tenancy;
using CodePunk.Conveyancing.Api.Features.Admin.Tenants;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAgentServices(builder.Configuration);
builder.Services.AddHttpClient();
builder.Services.AddDbContext<ConveyancingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=conveyancing.db"));
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ConveyancingDbContext>();
    // Dev convenience: create database/tables if not exist.
    db.Database.EnsureCreated();
    // Migration plan: replace EnsureCreated with db.Database.Migrate() once migrations are added.
}

app.UseSwagger();
app.UseSwaggerUI();

var api = app.MapGroup("/api");

// Tenant provisioning (no tenant required)
api.MapTenantAdminEndpoints();

// Resolve tenant for all other API routes
app.UseMiddleware<TenantResolverMiddleware>();

api.MapCreateConveyanceEndpoints();
api.MapGetConveyanceEndpoints();
api.MapCreateTitleEnquiryDraftEndpoints();
api.MapListDraftsEndpoints();
api.MapGetDraftEndpoints();
api.MapApproveDraftEndpoints();
api.MapRejectDraftEndpoints();
api.MapListOutboxEndpoints();
api.MapGetOutboxEndpoints();
api.MapCreateOutboxEndpoints();
api.MapMutateOutboxEndpoints();

app.MapHealthEndpoints();
app.MapAgentEndpoints();

app.Run();

// Expose Program for WebApplicationFactory in tests
public partial class Program { }
