using NUnit.Framework;
using CoffeeShop; 
using System.Collections.Generic;
using System.Linq;
using System; 

[TestFixture]
public class CustomerGetDrinkOrderTests
{
    [Test]
    public void TestNormalCustomerOrdersNormalRecipe()
    {
        //Setup
        Ingredient coffeeBean = new Ingredient("Coffee Bean", "...", IngredientType.CoffeeBean, false, 6, 1, 1, 0.5f);
        Recipe espressoRecipe = new Recipe("Espresso", 3.0f, 10f, EquipmentType.CoffeeMachine, 1, false).AddIngredientRequirement(coffeeBean, 1);
        Recipe supernaturalRecipe = new Recipe("Shadow Brew", 6.0f, 25f, EquipmentType.CoffeeMachine, 1, true).AddIngredientRequirement(coffeeBean, 1);
        var availableRecipes = new List<Recipe> { espressoRecipe, supernaturalRecipe };
        
        Dialogue dialogue = new Dialogue();
        dialogue.AddLine("Norm", "A regular coffee, please.");
        var normalCustomer = new NormalCustomer("Norm1", CustomerType.Student, "cust_student", dialogue, 1.0f);

        //Execute
        Recipe? orderedRecipe = normalCustomer.GetDrinkOrder(availableRecipes, 1);

        //Check
        Assert.IsNotNull(orderedRecipe, "Normal customer should have ordered a recipe.");
        Assert.IsFalse(orderedRecipe?.IsSupernatural, "Normal customer should not order a supernatural recipe.");
        Assert.AreEqual("Espresso", orderedRecipe?.RecipeName, "Normal customer should pick the available normal recipe.");
    }

    [Test]
    public void TestAlienCustomerOrdersNothing()
    {
        //Setup
        var anyRecipes = new List<Recipe>();
        Dialogue dialogue = new Dialogue();
        dialogue.AddLine("Alien", "Analyze.");
        var alienCustomer = new SupernaturalCustomer("Alien1", "alien_sprite", dialogue, SupernaturalCustomerType.Alien, CustomerMood.Calm, "");

        //Execute
        Recipe? orderedRecipe = alienCustomer.GetDrinkOrder(anyRecipes, 1);

        //Check
        Assert.IsNull(orderedRecipe, "Alien customer should return null, as they don't order standard recipes.");
    }

    [Test]
    public void TestGhostOrdersShadowBrewWhenAvailable()
    {
        //Setup
        Ingredient coffeeBean = new Ingredient("Coffee Bean", "...", IngredientType.CoffeeBean, false, 6, 1, 1, 0.5f);
        Recipe espressoRecipe = new Recipe("Espresso", 3.0f, 10f, EquipmentType.CoffeeMachine, 1, false).AddIngredientRequirement(coffeeBean, 1);
        Recipe shadowBrewRecipe = new Recipe("Shadow Brew", 6.0f, 25f, EquipmentType.CoffeeMachine, 1, true).AddIngredientRequirement(coffeeBean, 1);
        var availableRecipes = new List<Recipe> { espressoRecipe, shadowBrewRecipe };

        Dialogue dialogue = new Dialogue();
        dialogue.AddLine("Ghost", "Something from the beyond...");
        var ghostCustomer = new SupernaturalCustomer("Ghost1", "ghost_sprite", dialogue, SupernaturalCustomerType.Ghost, CustomerMood.Calm, "");

        //Execute
        Recipe? orderedRecipe = ghostCustomer.GetDrinkOrder(availableRecipes, 1);

        //Check
        Assert.IsNotNull(orderedRecipe, "Ghost should have ordered its preferred drink.");
        Assert.AreEqual("Shadow Brew", orderedRecipe?.RecipeName, "Ghost should prioritize 'Shadow Brew' when it is available.");
        Assert.IsTrue(orderedRecipe?.IsSupernatural);
    }

    [Test]
    public void TestGhostFallsBackToNormalRecipe()
    {
        //Setup
        Ingredient coffeeBean = new Ingredient("Coffee Bean", "...", IngredientType.CoffeeBean, false, 6, 1, 1, 0.5f);
        Recipe espressoRecipe = new Recipe("Espresso", 3.0f, 10f, EquipmentType.CoffeeMachine, 1, false).AddIngredientRequirement(coffeeBean, 1);
        var availableRecipes = new List<Recipe> { espressoRecipe }; // "Shadow Brew" is NOT available

        Dialogue dialogue = new Dialogue();
        dialogue.AddLine("Ghost", "I suppose this will have to do...");
        var ghostCustomer = new SupernaturalCustomer("Ghost2", "ghost_sprite", dialogue, SupernaturalCustomerType.Ghost, CustomerMood.Calm, "");

        //Execute
        Recipe? orderedRecipe = ghostCustomer.GetDrinkOrder(availableRecipes, 1);

        //Check
        Assert.IsNotNull(orderedRecipe, "Ghost should have ordered a fallback recipe.");
        Assert.IsFalse(orderedRecipe?.IsSupernatural, "Ghost's fallback order should be non-supernatural.");
        Assert.AreEqual("Espresso", orderedRecipe?.RecipeName, "Ghost should fall back to a normal recipe if 'Shadow Brew' is not available.");
    }

    [Test]
    public void TestFireMonsterOrdersFireyBrewWhenAvailable()
    {
        //Setup
        Ingredient coffeeBean = new Ingredient("Coffee Bean", "...", IngredientType.CoffeeBean, false, 6, 1, 1, 0.5f);
        Recipe fireyBrewRecipe = new Recipe("Firey Brew", 7.0f, 30f, EquipmentType.CoffeeMachine, 2, true).AddIngredientRequirement(coffeeBean, 1);
        Recipe latteRecipe = new Recipe("Latte", 4.0f, 15f, EquipmentType.CoffeeMachine, 1, false).AddIngredientRequirement(coffeeBean, 1);
        var availableRecipes = new List<Recipe> { latteRecipe, fireyBrewRecipe };

        Dialogue dialogue = new Dialogue();
        dialogue.AddLine("FireMonster", "Give me fuel!");
        var fireMonsterCustomer = new SupernaturalCustomer("FireM1", "firemonster_sprite", dialogue, SupernaturalCustomerType.FireMonster, CustomerMood.Agitated, "");

        //Execute
        Recipe? orderedRecipe = fireMonsterCustomer.GetDrinkOrder(availableRecipes, 2); // In this scenario Firey Brew is unlocked at level 2

        //Check
        Assert.IsNotNull(orderedRecipe, "FireMonster should have ordered its preferred drink.");
        Assert.AreEqual("Firey Brew", orderedRecipe?.RecipeName, "FireMonster should prioritize 'Firey Brew' when it is available.");
    }
    
    [Test]
    public void TestFireMonsterFallsBackToNormalRecipe()
    {
        //Setup
        Ingredient coffeeBean = new Ingredient("Coffee Bean", "...", IngredientType.CoffeeBean, false, 6, 1, 1, 0.5f);
        Recipe latteRecipe = new Recipe("Latte", 4.0f, 15f, EquipmentType.CoffeeMachine, 1, false).AddIngredientRequirement(coffeeBean, 1);
        var availableRecipes = new List<Recipe> { latteRecipe }; // "Firey Brew" is NOT available

        Dialogue dialogue = new Dialogue();
        dialogue.AddLine("FireMonster", "This lacks heat, but it will do.");
        var fireMonsterCustomer = new SupernaturalCustomer("FireM2", "firemonster_sprite", dialogue, SupernaturalCustomerType.FireMonster, CustomerMood.Agitated, "");

        //Execute
        Recipe? orderedRecipe = fireMonsterCustomer.GetDrinkOrder(availableRecipes, 1);

        //Check
        Assert.IsNotNull(orderedRecipe, "FireMonster should have ordered a fallback recipe.");
        Assert.IsFalse(orderedRecipe?.IsSupernatural, "FireMonster's fallback should be a normal recipe.");
        Assert.AreEqual("Latte", orderedRecipe?.RecipeName);
    }

    [Test]
    public void TestCustomerOrderVariety()
    {
        //Setup
        Ingredient coffeeBean = new Ingredient("Coffee Bean", "...", IngredientType.CoffeeBean, false, 6, 1, 1, 0.5f);
        Recipe espressoRecipe = new Recipe("Espresso", 3.0f, 10f, EquipmentType.CoffeeMachine, 1, false).AddIngredientRequirement(coffeeBean, 1);
        Recipe shadowBrewRecipe = new Recipe("Shadow Brew", 6.0f, 25f, EquipmentType.CoffeeMachine, 1, true).AddIngredientRequirement(coffeeBean, 1);
        var availableRecipes = new List<Recipe> { espressoRecipe, shadowBrewRecipe };
        
        Dialogue dialogue = new Dialogue();
        dialogue.AddLine("Test", "Test line.");

        var normalCust = new NormalCustomer("NormX", CustomerType.Tourist, "cust_tourist", dialogue, 1.2f);
        var ghostCust = new SupernaturalCustomer("GhostX", "ghost_sprite", dialogue, SupernaturalCustomerType.Ghost, CustomerMood.Calm, ""); 
        var alienCust = new SupernaturalCustomer("AlienX", "alien_sprite", dialogue, SupernaturalCustomerType.Alien, CustomerMood.Calm, "");
        
        var customers = new List<Customer> { normalCust, ghostCust, alienCust };
        var results = new List<Recipe?>();

        //Execute
        foreach (var cust in customers)
        {
            results.Add(cust.GetDrinkOrder(availableRecipes, 1));
        }

        //Check
        Assert.IsNotNull(results[0], "NormalCustomer should order.");
        Assert.AreEqual("Espresso", results[0]?.RecipeName, "NormalCustomer should select the normal recipe.");
        
        Assert.IsNotNull(results[1], "GhostCustomer should order.");
        Assert.AreEqual("Shadow Brew", results[1]?.RecipeName, "GhostCustomer should select its preferred supernatural recipe.");

        Assert.IsNull(results[2], "AlienCustomer should return null.");
    }
}