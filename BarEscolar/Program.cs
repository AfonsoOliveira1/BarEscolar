using BarEscolar.Data;
using BarEscolar.Models;
using BarEscolar.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// --- ATIVAR SESSÃO ---
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register all JSON stores and authentication
builder.Services.AddSingleton<JsonUserStore>();
builder.Services.AddSingleton<JsonMenuStore>();
builder.Services.AddSingleton<JsonProductStore>();
builder.Services.AddSingleton<JsonCategoryStore>();
builder.Services.AddSingleton<JsonOrderStore>();
builder.Services.AddScoped<Authentication>();

var app = builder.Build();

// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var userStore = scope.ServiceProvider.GetRequiredService<JsonUserStore>();
    DataSeeder.SeedAdmin(userStore);

    var menuStore = scope.ServiceProvider.GetRequiredService<JsonMenuStore>();

    if (!menuStore.GetAllWeeks().Any())
    {
        DateTime today = DateTime.Today;
        int daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        DateTime firstMonday = today.AddDays(daysUntilMonday);

        int menuId = 1;
        int weekId = 1;

        // Seed 2 weeks as example
        for (int weekOffset = 0; weekOffset < 2; weekOffset++)
        {
            DateTime weekStartDate = firstMonday.AddDays(7 * weekOffset);

            var week = new MenuWeek
            {
                id = weekId++,
                weekstart = weekStartDate.ToString("yyyy-MM-dd"),
                menuDays = new List<MenuDay>()
            };

            var dailyMenus = new List<(string MainA, string SoupA, string DessertA, string NotesA,
                                       string MainB, string SoupB, string DessertB, string NotesB)>
            {
                ("Frango grelhado com arroz e salada", "Sopa de legumes", "Maçã assada", "Clássico saudável e leve.",
                 "Tofu salteado com legumes e arroz integral", "Creme de abóbora", "Pera", "Opção vegan rica em proteína."),

                ("Bacalhau à Brás", "Caldo verde", "Gelatina", "Tradicional e saboroso.",
                 "Feijoada de legumes com arroz branco", "Sopa de grão-de-bico", "Banana", "Conforto vegan."),

                ("Carne de porco à alentejana", "Sopa de cenoura", "Laranja", "Prato típico português.",
                 "Caril de grão com leite de coco e arroz basmati", "Sopa de abóbora", "Maçã", "Sabor oriental e nutritivo."),

                ("Esparguete à bolonhesa", "Sopa de feijão verde", "Iogurte", "Clássico preferido dos alunos.",
                 "Esparguete com bolonhesa de lentilhas", "Sopa de ervilhas", "Mousse de fruta", "Alternativa vegan saborosa."),

                ("Filetes de peixe com arroz de tomate", "Sopa de peixe", "Bolo de cenoura", "Para terminar bem a semana.",
                 "Hambúrguer de grão e batata-doce", "Sopa de lentilhas", "Fruta da época", "Encerramento leve e nutritivo.")
            };

            for (int dayOffset = 0; dayOffset < 5; dayOffset++)
            {
                DateTime menuDate = weekStartDate.AddDays(dayOffset);
                var menu = dailyMenus[dayOffset];

                // Option A
                week.menuDays.Add(new MenuDay
                {
                    Id = menuId++,
                    menuweekid = week.id,
                    Date = menuDate,
                    option = "A",
                    MainDish = menu.MainA,
                    Soup = menu.SoupA,
                    Dessert = menu.DessertA,
                    Notes = menu.NotesA,
                    MaxSeats = 30
                });

                // Option B
                week.menuDays.Add(new MenuDay
                {
                    Id = menuId++,
                    menuweekid = week.id,
                    Date = menuDate,
                    option = "B",
                    MainDish = menu.MainB,
                    Soup = menu.SoupB,
                    Dessert = menu.DessertB,
                    Notes = menu.NotesB,
                    MaxSeats = 30
                });
            }

            menuStore.AddWeek(week);
        }
    }
}
app.UseSession();
// Configure middleware
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

app.Run();
