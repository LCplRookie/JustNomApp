using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class Program
{
    public static string MenuName; // Stores the name of the menu loaded from the file.
    public static List<PizzaRecipe> pizzaRecipes = new List<PizzaRecipe>(); // Stores pizza recipes loaded from the file.
    public static List<BurgerRecipe> burgerRecipes = new List<BurgerRecipe>(); // Stores burger recipes loaded from the file.
    public static Dictionary<string, int> toppings = new Dictionary<string, int>(); // Stores available toppings and their prices.
    public static Dictionary<string, int> garnishes = new Dictionary<string, int>(); // Stores available garnishes and their prices.

    public static void Main()
    {
        LoadMenuFromFile("menu.txt"); // Load menu data from the file.

        bool running = true; // Variable to control the main loop.
        while (running)
        {
            // Display main menu options.
            Console.WriteLine($"Welcome to {MenuName}! Brought to you by JustNom!");
            Console.WriteLine("1. View Saves");
            Console.WriteLine("2. Place Order");
            Console.WriteLine("3. Exit");
            Console.Write("Enter your choice: ");
            string choice = Console.ReadLine(); // Read user's choice.

            switch (choice)
            {
                case "1":
                    ViewSaves(); // View saved orders.
                    break;
                case "2":
                    PlaceOrder(); // Place a new order.
                    break;
                case "3":
                    running = false; // Exit the program.
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again."); // Invalid choice.
                    break;
            }
        }
    }

    // Load menu data from the specified file.
    public static void LoadMenuFromFile(string filename)
    {
        string menuData = File.ReadAllText(filename); // Read all text from the file.

        var lines = menuData.Split(new[] { Environment.NewLine }, StringSplitOptions.None); // Split the data into lines.
        MenuName = lines[0].Substring(lines[0].IndexOf(':') + 1).Trim(); // Extract the menu name.
        toppings = ParseItems(lines[1].Substring(lines[1].IndexOf(':') + 1)); // Parse toppings data.
        garnishes = ParseItems(lines[2].Substring(lines[2].IndexOf(':') + 1)); // Parse garnishes data.

        // Parse pizza and burger recipes from the remaining lines.
        for (int i = 3; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("Pizza:"))
            {
                pizzaRecipes.Add(ParsePizza(lines[i])); // Parse and add pizza recipe.
            }
            else if (lines[i].StartsWith("Burger:"))
            {
                burgerRecipes.Add(ParseBurger(lines[i])); // Parse and add burger recipe.
            }
        }
    }

    // View saved orders.
    public static void ViewSaves()
    {
        if (File.Exists("Saves.txt"))
        {
            Console.WriteLine("Saved Orders:");
            Console.WriteLine(File.ReadAllText("Saves.txt")); // Display saved orders.
        }
        else
        {
            Console.WriteLine("No saved orders."); // No saved orders found.
        }
    }

    // Place a new order.
    public static void PlaceOrder()
    {
        Console.WriteLine($"Place your order here at {MenuName}!"); // Display welcome message.

        Console.Write("Please enter your name: ");
        var customerName = Console.ReadLine(); // Read customer's name.

        Console.Write("Is this order for delivery (yes/no)? ");
        var isDelivery = Console.ReadLine().Trim().ToLower() == "yes"; // Determine if the order is for delivery.

        string address = null;
        if (isDelivery)
        {
            Console.Write("Please enter your address: ");
            address = Console.ReadLine(); // Read customer's address for delivery.
        }

        var order = new Order { CustomerName = customerName, IsDelivery = isDelivery, Address = address }; // Create a new order.

        bool addingItems = true; // Variable to control the item selection loop.
        while (addingItems)
        {
            Console.WriteLine("Please select an item to add to your order:");
            Console.WriteLine("1. Pizza");
            Console.WriteLine("2. Burger");
            Console.WriteLine("3. Finish adding items");
            var choice = Console.ReadLine(); // Read user's choice.

            switch (choice)
            {
                case "1":
                    AddPizzaToOrder(order); // Add a pizza to the order.
                    break;
                case "2":
                    AddBurgerToOrder(order); // Add a burger to the order.
                    break;
                case "3":
                    if (order.Items.Count == 0)
                    {
                        Console.WriteLine("You cannot complete the order without selecting any items. Please add at least one item."); // No items selected.
                    }
                    else
                    {
                        addingItems = false; // Exit item selection loop.
                    }
                    break;
                default:
                    Console.WriteLine("Invalid selection. Please try again."); // Invalid choice.
                    break;
            }
        }

        if (order.Items.Count > 0)
        {
            Console.WriteLine(order.GetOrderSummary()); // Display order summary.

            Console.Write("Do you want to save this order (yes/no)? ");
            var saveChoice = Console.ReadLine().Trim().ToLower();
            if (saveChoice == "yes")
            {
                SaveOrder(order); // Save the order.
            }
        }
    }

    // Save the order to a file.
    private static void SaveOrder(Order order)
    {
        string orderInfo = $"{DateTime.Now}: {order.GetOrderSummary()}\n"; // Format order information.
        string saveInfo = $"====================\n{orderInfo}====================\n"; // Add top and bottom borders.
        File.AppendAllText("Saves.txt", saveInfo); // Append order information to the file.
        Console.WriteLine("Order saved successfully."); // Display success message.
    }


    // Add a pizza to the order.
    private static void AddPizzaToOrder(Order order)
    {
        Console.WriteLine("Available Pizzas:");
        foreach (var pizza in pizzaRecipes)
        {
            Console.WriteLine($"{pizza.Name} - £{pizza.Price / 100.0:F2}"); // Display available pizzas.
        }

        Console.Write("Enter the name of the pizza you'd like to order: ");
        var pizzaName = Console.ReadLine();
        var selectedPizza = pizzaRecipes.FirstOrDefault(p => p.Name.Equals(pizzaName, StringComparison.OrdinalIgnoreCase));

        if (selectedPizza != null)
        {
            var toppingsToAdd = new List<string>();
            var toppingsToRemove = new List<string>();

            Console.WriteLine("Do you want to add or remove toppings, or keep the pizza as is? (add/remove/keep)");
            var decision = Console.ReadLine().Trim().ToLower();

            switch (decision)
            {
                case "add":
                    DisplayToppings();
                    Console.WriteLine("Enter the toppings you'd like to add (separated by commas): ");
                    var selectedPizzaToppings = Console.ReadLine().Split(',').Select(t => t.Trim()).ToList();
                    toppingsToAdd = selectedPizzaToppings.Where(t => toppings.ContainsKey(t)).ToList();
                    break;
                case "remove":
                    Console.WriteLine($"Current toppings on {selectedPizza.Name}: {string.Join(", ", selectedPizza.Toppings)}");
                    Console.WriteLine("Enter the toppings you'd like to remove (separated by commas): ");
                    toppingsToRemove = Console.ReadLine().Split(',').Select(t => t.Trim()).ToList();
                    break;
                case "keep":
                    break;
                default:
                    Console.WriteLine("Invalid selection. Please try again.");
                    break;
            }

            order.AddItem(new Pizza(selectedPizza.Name, selectedPizza.Toppings, toppingsToAdd, toppingsToRemove));
        }
        else
        {
            Console.WriteLine("Pizza not found. Please try again.");
        }
    }

    // Add a burger to the order.
    private static void AddBurgerToOrder(Order order)
    {
        Console.WriteLine("Available Burgers:");
        foreach (var burger in burgerRecipes)
        {
            Console.WriteLine($"{burger.Name} - £{burger.Price / 100.0:F2}"); // Display available burgers.
        }

        Console.Write("Enter the name of the burger you'd like to order: ");
        var burgerName = Console.ReadLine();
        var selectedBurger = burgerRecipes.FirstOrDefault(b => b.Name.Equals(burgerName, StringComparison.OrdinalIgnoreCase));

        if (selectedBurger != null)
        {
            var garnishesToAdd = new List<string>();
            var garnishesToRemove = new List<string>();

            Console.WriteLine("Do you want to add or remove garnishes, or keep the burger as is? (add/remove/keep)");
            var decision = Console.ReadLine().Trim().ToLower();

            switch (decision)
            {
                case "add":
                    DisplayGarnishes();
                    Console.WriteLine("Enter the garnishes you'd like to add (separated by commas): ");
                    var selectedBurgerGarnishes = Console.ReadLine().Split(',').Select(g => g.Trim()).ToList();
                    garnishesToAdd = selectedBurgerGarnishes.Where(g => garnishes.ContainsKey(g)).ToList();
                    break;
                case "remove":
                    Console.WriteLine($"Current garnishes on {selectedBurger.Name}: {string.Join(", ", selectedBurger.Garnishes)}");
                    Console.WriteLine("Enter the garnishes you'd like to remove (separated by commas): ");
                    garnishesToRemove = Console.ReadLine().Split(',').Select(g => g.Trim()).ToList();
                    break;
                case "keep":
                    break;
                default:
                    Console.WriteLine("Invalid selection. Please try again.");
                    break;
            }

            order.AddItem(new Burger(selectedBurger.Name, selectedBurger.Garnishes, garnishesToAdd, garnishesToRemove));
        }
        else
        {
            Console.WriteLine("Burger not found. Please try again.");
        }
    }

    // Display available toppings.
    private static void DisplayToppings()
    {
        Console.WriteLine("Available Toppings:");
        foreach (var topping in toppings)
        {
            Console.WriteLine($"{topping.Key} - £{topping.Value / 100.0:F2}"); // Display available toppings and their prices.
        }
    }

    // Display available garnishes.
    private static void DisplayGarnishes()
    {
        Console.WriteLine("Available Garnishes:");
        foreach (var garnish in garnishes)
        {
            Console.WriteLine($"{garnish.Key} - £{garnish.Value / 100.0:F2}"); // Display available garnishes and their prices.
        }
    }

    // Parse pizza recipe from a string line.
    public static PizzaRecipe ParsePizza(string line)
    {
        var nameMatch = Regex.Match(line, "Name:(.*?),");
        var toppingsMatch = Regex.Match(line, "Toppings:\\[(.*?)\\]");
        var priceMatch = Regex.Match(line, "Price:(\\d+)");

        var name = nameMatch.Groups[1].Value;
        var toppingsList = toppingsMatch.Groups[1].Value.Split(',').Select(t => t.Trim()).ToList();
        var price = int.Parse(priceMatch.Groups[1].Value);

        return new PizzaRecipe { Name = name, Toppings = toppingsList, Price = price }; // Create and return a new pizza recipe object.
    }

    // Parse burger recipe from a string line.
    public static BurgerRecipe ParseBurger(string line)
    {
        var nameMatch = Regex.Match(line, "Name:(.*?),");
        var garnishesMatch = Regex.Match(line, "Garnishes:\\[(.*?)\\]");
        var priceMatch = Regex.Match(line, "Price:(\\d+)");

        var name = nameMatch.Groups[1].Value;
        var garnishesList = garnishesMatch.Groups[1].Value.Split(',').Select(g => g.Trim()).ToList();
        var price = int.Parse(priceMatch.Groups[1].Value);

        return new BurgerRecipe { Name = name, Garnishes = garnishesList, Price = price }; // Create and return a new burger recipe object.
    }

    // Parse items (toppings/garnishes) from a string line.
    public static Dictionary<string, int> ParseItems(string line)
    {
        var items = new Dictionary<string, int>();
        var matches = Regex.Matches(line, "<(.*?),(\\d+)>");

        foreach (Match match in matches)
        {
            var itemName = match.Groups[1].Value;
            var itemPrice = int.Parse(match.Groups[2].Value);
            items[itemName] = itemPrice; // Add item and its price to the dictionary.
        }

        return items; // Return the dictionary containing items and their prices.
    }
}

// Represents an order.
public class Order
{
    public string CustomerName { get; set; } // Customer's name.
    public bool IsDelivery { get; set; } // Indicates if the order is for delivery.
    public string Address { get; set; } // Customer's address for delivery.
    public List<FoodItem> Items { get; } = new List<FoodItem>(); // List of items in the order.

    // Add an item to the order.
    public void AddItem(FoodItem item)
    {
        Items.Add(item);
    }

    // Get the summary of the order.
    public string GetOrderSummary()
    {
        var summary = $"Order for {CustomerName}\n"; // Initialize order summary.
        var total = 0; // Initialize total price.

        // Calculate total price and construct order summary.
        foreach (var item in Items)
        {
            total += item.GetPrice();
            summary += $"{item}\n"; // Add item to the summary.
        }

        // Add delivery charge if applicable.
        if (IsDelivery)
        {
            var deliveryCharge = total > 2000 ? 0 : 200; // £2.00 in pence
            summary += $"Delivery Charge: £{deliveryCharge / 100.0:F2}\n"; // Add delivery charge to the summary.
            total += deliveryCharge; // Update total price.
        }

        summary += $"Total: £{total / 100.0:F2}"; // Add total price to the summary.
        return summary; // Return the order summary.
    }
}

// Represents a food item.
public abstract class FoodItem
{
    public string Name { get; set; } // Name of the item.
    public List<string> BaseIngredients { get; set; } // Base ingredients of the item.
    public List<string> ExtraIngredients { get; set; } // Extra ingredients added to the item.
    public int BasePrice { get; set; } // Base price of the item.

    // Abstract method to get the price of the item.
    public abstract int GetPrice();

    // Override ToString method to display the item information.
    public override string ToString()
    {
        return $"{Name}, Price: £{GetPrice() / 100.0:F2}";
    }
}

// Represents a pizza.
public class Pizza : FoodItem
{
    public List<string> ToppingsToRemove { get; set; } // Toppings to be removed from the pizza.

    public Pizza(string name, List<string> baseIngredients, List<string> extraIngredients, List<string> toppingsToRemove)
    {
        Name = name;
        BaseIngredients = baseIngredients;
        ExtraIngredients = extraIngredients;
        ToppingsToRemove = toppingsToRemove;
        BasePrice = Program.pizzaRecipes.FirstOrDefault(p => p.Name == name)?.Price ?? 0; // Get base price from the recipes.
    }

    // Calculate the price of the pizza.
    public override int GetPrice()
    {
        var price = BasePrice;
        price += ExtraIngredients.Sum(ingredient => Program.toppings.TryGetValue(ingredient, out var value) ? value : 0); // Add price of extra toppings.
        return price;
    }

    // Override ToString method to display pizza information.
    public override string ToString()
    {
        var extraToppings = ExtraIngredients.Any() ? $", Extra Toppings: {string.Join(", ", ExtraIngredients)}" : "";
        var removedToppings = ToppingsToRemove.Any() ? $", Removed Toppings: {string.Join(", ", ToppingsToRemove)}" : "";
        return $"Pizza: {Name}{extraToppings}{removedToppings}, Price: £{GetPrice() / 100.0:F2}";
    }
}

// Represents a burger.
public class Burger : FoodItem
{
    public List<string> GarnishesToRemove { get; set; } // Garnishes to be removed from the burger.

    public Burger(string name, List<string> baseIngredients, List<string> extraIngredients, List<string> garnishesToRemove)
    {
        Name = name;
        BaseIngredients = baseIngredients;
        ExtraIngredients = extraIngredients;
        GarnishesToRemove = garnishesToRemove;
        BasePrice = Program.burgerRecipes.FirstOrDefault(b => b.Name == name)?.Price ?? 0; // Get base price from the recipes.
    }

    // Calculate the price of the burger.
    public override int GetPrice()
    {
        var price = BasePrice;
        price += ExtraIngredients.Sum(ingredient => Program.garnishes.TryGetValue(ingredient, out var value) ? value : 0); // Add price of extra garnishes.
        return price;
    }

    // Override ToString method to display burger information.
    public override string ToString()
    {
        var extraGarnishes = ExtraIngredients.Any() ? $", Extra Garnishes: {string.Join(", ", ExtraIngredients)}" : "";
        var removedGarnishes = GarnishesToRemove.Any() ? $", Removed Garnishes: {string.Join(", ", GarnishesToRemove)}" : "";
        return $"Burger: {Name}{extraGarnishes}{removedGarnishes}, Price: £{GetPrice() / 100.0:F2}";
    }
}

// Represents a pizza recipe.
public class PizzaRecipe
{
    public string Name { get; set; } // Name of the pizza.
    public List<string> Toppings { get; set; } // Toppings of the pizza.
    public int Price { get; set; } // Price of the pizza.
}

// Represents a burger recipe.
public class BurgerRecipe
{
    public string Name { get; set; } // Name of the burger.
    public List<string> Garnishes { get; set; } // Garnishes of the burger.
    public int Price { get; set; } // Price of the burger.
}
