using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using LibrarySystem.DataAccess.Config;
using LibrarySystem.DataAccess.Context;
using LibrarySystem.DataAccess.Repositories;

using LibrarySystem.Business.Services;

namespace LibrarySystem.Presentation.DependencyInjection
{
    public static class ServiceRegistration
    {
        public static IServiceCollection ConfigureServices(string connectionString)
        {
           
            //  dependency injection
            var services = new ServiceCollection();

            //configure dbcontext with connection 
            services.AddDbContext<ApplicationDbContext>(options => 
            options.UseNpgsql(connectionString));

            //repositories
            services.AddScoped<IAdminRepository, AdminRepository>();
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IBookCategoryRepository, BookCategoryRepository>();
            services.AddScoped<IMemberRepository, MemberRepository>();
            services.AddScoped<IBorrowingRulesRepository, BorrowingRulesRepository>();
            services.AddScoped<IBorrowingRepository, BorrowingRepository>();
            services.AddScoped<IBookCopyRepository, BookCopyRepository>();
            services.AddScoped<IFineRepository, FineRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();

            //services
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IBookCategoryService, BookCategoryService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IBorrowingRulesService, BorrowingRulesService>();
            services.AddScoped<IBorrowingService, BorrowingService>();
            services.AddScoped<IBookCopyService, BookCopyService>();
            services.AddScoped<IFineService, FineService>();
            services.AddScoped<IReportService, ReportService>();
            

            return services;

        }
    
    }
}