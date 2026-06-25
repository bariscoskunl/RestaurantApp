using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantApp.Common.Models
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager)
        {
            // Identity pakaetinden gelir
            // roleManager -> rol işlemlerinde kullanılır
            // userManager -> user işlemlerinde kullanılır.

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = { "Admin", "User", "Garson" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }


            // Admin Kullanıcısı
            string adminEmail = "admin@domain.com";   
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // User Kullanıcısı
            string userEmail = "user@domain.com";
            if (await userManager.FindByEmailAsync(userEmail) == null)
            {
                var normalUser = new ApplicationUser
                {
                    UserName =userEmail,
                    Email =userEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(normalUser, "User123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(normalUser, "User");
                }
            }

            // Garson Kullanıcısı
            string garsonEmail = "garson@domain.com";
            if (await userManager.FindByEmailAsync(garsonEmail) == null)
            {
                var garsonUser = new ApplicationUser
                {
                    UserName=garsonEmail,
                    Email=garsonEmail,
                    EmailConfirmed=true
                };

                var result = await userManager.CreateAsync(garsonUser, "Garson123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(garsonUser, "Garson");
                }
            }
        }
    }
}
