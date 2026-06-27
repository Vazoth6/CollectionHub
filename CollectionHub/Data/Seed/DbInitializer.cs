using CollectionHub.Data.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CollectionHub.Data.Seed
{
    // <summary>
    // Classe responsável por inicializar a base de dados com dados de teste
    // </summary>
    public static class DbInitializer
    {   
        // <summary>
        // Inicializa a base de dados com dados de seed
        // </summary>
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {

            Console.WriteLine("=== INICIANDO DB INITIALIZER ===");

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Aplica migrações pendentes (se existirem)
            Console.WriteLine("A aplicar migrações...");
            await context.Database.MigrateAsync();

            // =========================
            // 1. Cria Roles
            // =========================
            Console.WriteLine("A criar roles...");
            await CreateRolesAsync(roleManager);

            // =========================
            // 2. Cria Utilizadores de Teste
            // =========================
            Console.WriteLine("A criar utilizadores...");
            await CreateTestUsersAsync(userManager, context);

            // =========================
            // 3. Adiciona Categorias
            // =========================
            Console.WriteLine("A adicionar categorias...");
            await AddCategoriesAsync(context);

            // =========================
            // 4. Adiciona Items
            // =========================
            Console.WriteLine("A adicionar items...");
            await AddItemsAsync(context);

            Console.WriteLine("=== DB INITIALIZER CONCLUÍDO ===");
        }

        private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Vendedor", "Utilizador" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task CreateTestUsersAsync(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            // Admin User
            var adminEmail = "admin@collectionhub.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                    var adminMyUser = new MyUser
                    {
                        Name = "Administrador",
                        Role = "Admin",
                        RegisterDate = DateTime.Now,
                        CellPhone = "+351912345678",
                        UserID = adminUser.Id
                    };
                    context.MyUsers.Add(adminMyUser);
                }
            }

            // Vendedor User
            var sellerEmail = "vendedor@collectionhub.com";
            if (await userManager.FindByEmailAsync(sellerEmail) == null)
            {
                var sellerUser = new IdentityUser
                {
                    UserName = sellerEmail,
                    Email = sellerEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(sellerUser, "Vendedor123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(sellerUser, "Vendedor");

                    var sellerMyUser = new MyUser
                    {
                        Name = "João Vendedor",
                        Role = "Vendedor",
                        RegisterDate = DateTime.Now,
                        CellPhone = "+351912345679",
                        UserID = sellerUser.Id
                    };
                    context.MyUsers.Add(sellerMyUser);
                }
            }

            // Utilizador Normal
            var userEmail = "utilizador@collectionhub.com";
            if (await userManager.FindByEmailAsync(userEmail) == null)
            {
                var normalUser = new IdentityUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(normalUser, "User123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(normalUser, "Utilizador");

                    var normalMyUser = new MyUser
                    {
                        Name = "Maria Utilizadora",
                        Role = "Utilizador",
                        RegisterDate = DateTime.Now,
                        CellPhone = "+351912345680",
                        UserID = normalUser.Id
                    };
                    context.MyUsers.Add(normalMyUser);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task AddCategoriesAsync(ApplicationDbContext context)
        {
            var categories = new List<Category>
            {
                new Category { Name = "Carta Pokémon" },
                new Category { Name = "Carta Yu-gi-oh" },
                new Category { Name = "Carta Invizimal" },
                new Category { Name = "Videojogo" },
                new Category { Name = "Jogo de Tabuleiro" },
                new Category { Name = "Moeda" }
            };

            foreach (var category in categories)
            {
                if (!context.Categories.Any(c => c.Name == category.Name))
                {
                    context.Categories.Add(category);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task AddItemsAsync(ApplicationDbContext context)
        {
            // Obter as categorias
            var categoriaPokemon = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Carta Pokémon");
            var categoriaYugioh = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Carta Yu-gi-oh");
            var categoriaInvizimal = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Carta Invizimal");
            var categoriaVideoJogo = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Videojogo");
            var categoriaJogoTabuleiro = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Jogo de Tabuleiro");
            var categoriaMoeda = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Moeda");

            // Obter o vendedor (utilizador com Role "Vendedor")
            var identityUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "vendedor@collectionhub.com");
            var vendedor = await context.MyUsers.FirstOrDefaultAsync(m => m.UserID == identityUser.Id);

            if (vendedor == null) return;

            var items = new List<Item>
            {
                // Cartas Pokémon
                new Item
                {
                    Name = "Pikachu 1st Edition",
                    Description = "Carta rara do Pikachu em excelente estado. 1ª edição, holográfica.",
                    Price = 150.00m,
                    Status = "Disponível",
                    CategoryId = categoriaPokemon?.Id ?? 1,
                    SubmittedAt = DateTime.Now,
                    ImageUrl = "/images/items/pikachu.jpg"
                },
                new Item
                {
                    Name = "Charizard Holo",
                    Description = "Charizard holográfico da coleção base. Estado de conservação: Near Mint.",
                    Price = 350.00m,
                    Status = "Disponível",
                    CategoryId = categoriaPokemon?.Id ?? 1,
                    SubmittedAt = DateTime.Now,
                    ImageUrl = "/images/items/charizard.jpg"
                },
                new Item
                {
                    Name = "Mewtwo EX",
                    Description = "Mewtwo EX em perfeito estado. Carta de edição limitada.",
                    Price = 89.99m,
                    Status = "Disponível",
                    CategoryId = categoriaPokemon?.Id ?? 1,
                    SubmittedAt = DateTime.Now,
                    ImageUrl = "/images/items/mewtwo.jpg"
                },

                // Cartas Yu-gi-oh
                new Item
                {
                    Name = "Dragão Branco de Olhos Azuis",
                    Description = "Carta lendária do Dragão Branco de Olhos Azuis. 1ª edição.",
                    Price = 120.00m,
                    Status = "Disponível",
                    CategoryId = categoriaYugioh?.Id ?? 2,
                    SubmittedAt = DateTime.Now,
                    ImageUrl = "/images/items/blue-eyes.jpg"
                },
                new Item
                {
                    Name = "Mago Negro",
                    Description = "Mago Negro em estado Near Mint. Carta icónica do jogo.",
                    Price = 75.00m,
                    Status = "Disponível",
                    CategoryId = categoriaYugioh?.Id ?? 2,
                    SubmittedAt = DateTime.Now,
                    ImageUrl = "/images/items/dark-magician.jpg"
                },
                new Item
                {
                    Name = "Exodia, o Proibido",
                    Description = "Peça do Exodia. Completa o conjunto!",
                    Price = 200.00m,
                    Status = "Disponível",
                    CategoryId = categoriaYugioh?.Id ?? 2,
                    SubmittedAt = DateTime.Now,
                    ImageUrl = "/images/items/exodia.jpg"
                },

                // Cartas Invizimal
                new Item
                {
                    Name = "Nexomon Starter Pack",
                    Description = "Pack inicial de cartas Invizimal com 50 cartas.",
                    Price = 25.00m,
                    Status = "Disponível",
                    CategoryId = categoriaInvizimal?.Id ?? 3,
                    SubmittedAt = DateTime.Now,
                    ImageUrl = "/images/items/invizimal-pack.jpg"
                },
                new Item
                {
                    Name = "Invizimal - Dragão Lendário",
                    Description = "Carta rara do Dragão Lendário. Edição limitada.",
                    Price = 45.00m,
                    Status = "Disponível",
                    CategoryId = categoriaInvizimal?.Id ?? 3,
                    SubmittedAt = DateTime.Now,
                    ImageUrl = "/images/items/dragon-card.jpg"
                },

                // Videojogos
                new Item
                {
                    Name = "The Legend of Zelda: Tears of the Kingdom",
                    Description = "Nintendo Switch. Jogo completo com caixa e manual.",
                    Price = 55.00m,
                    Status = "Disponível",
                    CategoryId = categoriaVideoJogo?.Id ?? 4,
                    SubmittedAt = DateTime.Now,
                    ImageUrl = "/images/items/zelda.jpg"
                },
                new Item
                {
                    Name = "Elden Ring - PS5",
                    Description = "Elden Ring para PlayStation 5. Estado impecável.",
                    Price = 45.00m,
                    Status = "Disponível",
                    CategoryId = categoriaVideoJogo?.Id ?? 4,
                    SubmittedAt = DateTime.Now,
                    ImageUrl = "/images/items/elden-ring.jpg"
                },
                new Item
                {
                    Name = "Super Mario Odyssey",
                    Description = "Super Mario Odyssey para Nintendo Switch. Completo.",
                    Price = 40.00m,
                    Status = "Disponível",
                    CategoryId = categoriaVideoJogo?.Id ?? 4,
                    SubmittedAt = DateTime.Now,
                    ImageUrl = "/images/items/mario.jpg"
                },

                // Jogos de Tabuleiro
                new Item
                {
                    Name = "Catan - O Jogo",
                    Description = "Jogo de tabuleiro Catan completo. Todas as peças incluídas.",
                    Price = 35.00m,
                    Status = "Disponível",
                    CategoryId = categoriaJogoTabuleiro?.Id ?? 5,
                    SubmittedAt = DateTime.Now,
                    ImageUrl = "/images/items/catan.jpg"
                },
                new Item
                {
                    Name = "Ticket to Ride",
                    Description = "Ticket to Ride Europe. Jogo em ótimo estado.",
                    Price = 30.00m,
                    Status = "Disponível",
                    CategoryId = categoriaJogoTabuleiro?.Id ?? 5,
                    SubmittedAt = DateTime.Now,
                    ImageUrl = "/images/items/ticket-to-ride.jpg"
                },

                // Moedas
                new Item
                {
                    Name = "Moeda de Ouro 100 Escudos - 1998",
                    Description = "Moeda comemorativa da Expo 98. Excelente estado de conservação.",
                    Price = 85.00m,
                    Status = "Disponível",
                    CategoryId = categoriaMoeda?.Id ?? 6,
                    SubmittedAt = DateTime.Now,
                    ImageUrl = "/images/items/coin-100.jpg"
                },
                new Item
                {
                    Name = "Moeda de Prata 2.5 Euros",
                    Description = "Moeda de prata comemorativa. Tiragem limitada.",
                    Price = 60.00m,
                    Status = "Disponível",
                    CategoryId = categoriaMoeda?.Id ?? 6,
                    SubmittedAt = DateTime.Now,
                    ImageUrl = "/images/items/silver-coin.jpg"
                }
            };

            foreach (var item in items)
            {
                if (!context.Items.Any(i => i.Name == item.Name))
                {
                    context.Items.Add(item);
                    await context.SaveChangesAsync();

                    // Associa o item ao vendedor (UserItem)
                    var userItem = new UserItem
                    {
                        UserId = vendedor.Id,
                        ItemId = item.Id
                    };
                    context.UserItems.Add(userItem);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
