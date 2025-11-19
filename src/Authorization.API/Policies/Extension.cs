//using Microsoft.AspNetCore.Authorization;
//using Microsoft.Extensions.DependencyInjection;

//namespace IronExchange.Authorization
//{
//    public static class AuthorizationExtensions
//    {
//        public static IServiceCollection AddIronExchangeAuthorization(this IServiceCollection services)
//        {
//            services.AddAuthorization(options =>
//            {
//                // مثال: Catalog
//                options.AddPolicy(Permissions.Catalog.Read, policy =>
//                    policy.RequireClaim("permission", Permissions.Catalog.Read));

//                options.AddPolicy(Permissions.Catalog.Write, policy =>
//                    policy.RequireClaim("permission", Permissions.Catalog.Write));

//                // مثال: Ordering
//                options.AddPolicy(Permissions.Ordering.Read, policy =>
//                    policy.RequireClaim("permission", Permissions.Ordering.Read));

//                options.AddPolicy(Permissions.Ordering.Write, policy =>
//                    policy.RequireClaim("permission", Permissions.Ordering.Write));

//                // هر API جدید = اضافه کردن Policy اینجا
//            });

//            return services;
//        }
//    }

//    public static class Permissions
//    {
//        public static class Catalog
//        {
//            public const string Read = "catalog.read";
//            public const string Write = "catalog.write";
//        }

//        public static class Ordering
//        {
//            public const string Read = "ordering.read";
//            public const string Write = "ordering.write";
//        }

//        // بقیه سرویس‌ها...
//    }
//}
