using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using SubscriptionApp.Data;
using SubscriptionApp.Models;

namespace SubscriptionApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Configure PostgreSQL
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Configure Redis
            builder.Services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(builder.Configuration.GetSection("Redis:Connection").Value ?? throw new InvalidOperationException("Redis connection string not found.")));

            // Configure MassTransit with RabbitMQ
            builder.Services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(builder.Configuration.GetSection("RabbitMQ:Host").Value, h =>
                    {
                        h.Username(builder.Configuration.GetSection("RabbitMQ:Username").Value!);
                        h.Password(builder.Configuration.GetSection("RabbitMQ:Password").Value!);
                    });
                    cfg.ConfigureEndpoints(context);
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Publish a message on startup
            app.Lifetime.ApplicationStarted.Register(() =>
            {
                var bus = app.Services.GetRequiredService<IBus>();
                bus.Publish(new Message("Hello, MassTransit!")).GetAwaiter().GetResult();
            });

            app.Run();
        }
    }
}
