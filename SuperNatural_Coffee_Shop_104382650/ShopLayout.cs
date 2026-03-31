// ShopLayout.cs
using System.Collections.Generic;
using System.Linq;
using System;
using Raylib_cs;
using System.IO;

namespace CoffeeShop
{
    /// <summary>
    /// Manages the physical layout and customer flow of the coffee shop.
    /// This includes handling the customer queue, the placement and state of equipment stations,
    /// and the logic for spawning new customers based on game progression and rules.
    /// </summary>
    public class ShopLayout
    {
        /// <summary>
        /// The queue of customers currently waiting for service.
        /// </summary>
        private readonly Queue<Customer> _customerQueue;
        /// <summary>
        /// A list of all interactive stations (e.g., for coffee machines) within the shop.
        /// </summary>
        private readonly List<Station> _stations;

        /// <summary>
        /// The maximum number of customers allowed in the waiting queue at any given time.
        /// </summary>
        private readonly int _maxQueueSize = 3;
        /// <summary>
        /// Timer to track when the next customer spawn attempt should occur.
        /// </summary>
        private float _customerSpawnTimer;
        /// <summary>
        /// The randomly determined delay before the next customer spawn attempt.
        /// </summary>
        private float _nextSpawnDelay;
        /// <summary>
        /// Random number generator for various probabilistic events, like spawn delays and customer type selection.
        /// </summary>
        private Random _random = new Random();
        /// <summary>
        /// Timer for a cooldown period after a customer leaves, preventing immediate re-spawning in that slot.
        /// </summary>
        private float _spawnCooldownTimer;
        /// <summary>
        /// Defines the duration of the spawn cooldown when a customer leaves a queue slot.
        /// </summary>
        private const float SPAWN_COOLDOWN_ON_LEAVE = 2.0f;
        /// <summary>
        /// Tracks the type of the last supernatural customer spawned to help vary subsequent supernatural spawns.
        /// </summary>
        private SupernaturalCustomerType? _lastSpawnedSupernaturalType = null;

        /// <summary>
        /// List of sprite prefixes for Student type normal customers.
        /// </summary>
        private readonly List<string> _studentSpritePrefixes = new List<string> { "cust2", "cust3", "cust5", "cust10" };
        /// <summary>
        /// List of sprite prefixes for Tourist type normal customers.
        /// </summary>
        private readonly List<string> _touristSpritePrefixes = new List<string> { "cust4", "cust7", "cust9" };
        /// <summary>
        /// List of sprite prefixes for OfficeWorker type normal customers.
        /// </summary>
        private readonly List<string> _officeWorkerSpritePrefixes = new List<string> { "cust1", "cust6", "cust8" };

        /// <summary>
        /// A collection of generic phrases customers might say when initially placing an order or greeting.
        /// </summary>
        private readonly List<string> _genericOrderRequests = new List<string> {
            "I'd like a drink, please.",
            "Feeling a bit thirsty!",
            "Could I get something from your menu?",
            "What do you recommend today?",
            "One drink, coming right up... for me!",
            "Surprise me with something nice!",
            "Hello! One beverage, please.",
            "I could use a refreshment.",
            "I need something cool and delicious!",
            "Pour me a glass of something amazing!",
            "I'll take whatever is buzzing today!",
            "I could use a sip of something special."
        };

        /// <summary>
        /// Gets the queue of customers currently waiting for service.
        /// </summary>
        public Queue<Customer> CustomerQueue => _customerQueue;
        /// <summary>
        /// Gets the list of interactive stations (e.g., for coffee machines) within the shop.
        /// </summary>
        public List<Station> Stations => _stations;
        /// <summary>
        /// Gets the maximum number of customers allowed in the queue at one time.
        /// </summary>
        public int MaxQueueSize => _maxQueueSize;

        public List<Customer> AllCustomersInShop(Customer? interactingCustomer)
        {
            var allCustomers = new List<Customer>(_customerQueue);
            if (interactingCustomer != null && !allCustomers.Contains(interactingCustomer))
            {
                allCustomers.Add(interactingCustomer);
            }
            return allCustomers;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShopLayout"/> class.
        /// Sets up the customer queue, station list, and initializes customer spawning timers.
        /// </summary>
        public ShopLayout()
        {
            _customerQueue = new Queue<Customer>();
            _stations = new List<Station>();
            _customerSpawnTimer = 0f;
            _nextSpawnDelay = GetRandomSpawnDelay();
            _spawnCooldownTimer = 0f;
            System.Console.WriteLine("ShopLayout initialized.");
        }

        /// <summary>
        /// Calculates and returns a random delay time for the next customer spawn.
        /// </summary>
        /// <param name="minSeconds">The minimum possible delay in seconds before the next spawn attempt.</param>
        /// <param name="maxSeconds">The maximum possible delay in seconds before the next spawn attempt.</param>
        /// <returns>A float representing the calculated random spawn delay in seconds.</returns>
        private float GetRandomSpawnDelay(float minSeconds = 5f, float maxSeconds = 15f)
        {
            return (float)(_random.NextDouble() * (maxSeconds - minSeconds) + minSeconds);
        }

        /// <summary>
        /// Adds a specified customer to the waiting queue if there is space available.
        /// Logs the action or reason for not adding (e.g., queue full, null customer).
        /// </summary>
        /// <param name="customer">The <see cref="Customer"/> object to add to the queue. Should not be null.</param>
        public void AddCustomerToQueue(Customer customer)
        {
            if (customer != null && _customerQueue.Count < _maxQueueSize)
            {
                _customerQueue.Enqueue(customer);
                System.Console.WriteLine($"Customer {customer.CustomerID} added to queue. Queue size: {_customerQueue.Count}");
            }
            else if (customer == null)
            {
                System.Console.WriteLine("Attempted to add a null customer to the queue.");
            }
            else
            {
                System.Console.WriteLine($"Queue is full. Customer {customer.CustomerID} cannot be added.");
            }
        }

        /// <summary>
        /// Retrieves and removes the next customer from the front of the queue.
        /// </summary>
        /// <returns>The <see cref="Customer"/> at the front of the queue, or <c>null</c> if the queue is empty.</returns>
        public Customer? GetNextCustomer()
        {
            if (_customerQueue.Count > 0)
            {
                Customer nextCustomer = _customerQueue.Dequeue();
                System.Console.WriteLine($"Customer {nextCustomer.CustomerID} dequeued. Queue size: {_customerQueue.Count}");
                return nextCustomer;
            }
            System.Console.WriteLine("Customer queue is empty.");
            return null;
        }

        /// <summary>
        /// Finds and returns a station that is designated for a specific type of equipment.
        /// Currently, this method is hardcoded to search for a <see cref="EquipmentType.CoffeeMachine"/>.
        /// </summary>
        /// <param name="type">The <see cref="EquipmentType"/> to search for at available stations.</param>
        /// <returns>The first <see cref="Station"/> found that holds the specified equipment type, or <c>null</c> if no such station is found.</returns>
        public Station? GetStation(EquipmentType type)
        {
            // Note: This implementation currently only looks for a CoffeeMachine, regardless of the 'type' parameter.
            // Future enhancements might involve a more dynamic lookup based on the 'type' parameter.
            Station? foundStation = _stations.FirstOrDefault(s => s.AssignedEquipment?.Type == EquipmentType.CoffeeMachine);
            if (foundStation == null)
            {
                Console.WriteLine($"WARNING: No station found for EquipmentType.CoffeeMachine in ShopLayout.GetStation(). (Searched for: {type})");
            }
            return foundStation;
        }

        /// <summary>
        /// Gets the equipment currently assigned to a given station.
        /// </summary>
        /// <param name="station">The <see cref="Station"/> to inspect. Can be null.</param>
        /// <returns>The <see cref="Equipment"/> assigned to the station, or <c>null</c> if the station is null or has no equipment.</returns>
        public Equipment? GetEquipmentAtStation(Station? station)
        {
            if (station != null)
            {
                return station.AssignedEquipment;
            }
            System.Console.WriteLine("ShopLayout.GetEquipmentAtStation: Provided station was null.");
            return null;
        }

        /// <summary>
        /// Creates and returns a new instance of a normal customer based on the specified type.
        /// This method initializes the customer with a unique ID, selects a sprite, sets up initial dialogue,
        /// and applies a patience modifier.
        /// </summary>
        /// <param name="customerTypeToSpawn">The <see cref="CustomerType"/> of the normal customer to be spawned.</param>
        /// <param name="gameManager">A reference to the <see cref="GameManager"/>, used for context such as loading customer textures.</param>
        /// <returns>A newly created <see cref="NormalCustomer"/> instance, or <c>null</c> if spawning fails (though current implementation always attempts to create a customer or defaults).</returns>
        public Customer? SpawnCustomer(CustomerType customerTypeToSpawn, GameManager gameManager)
        {
            string newCustomerId = $"Cust_{System.Guid.NewGuid().ToString().Substring(0, 4)}";
            string spritePrefix = "";
            Dialogue dialogueSet = new Dialogue();
            float patienceModifier = 1.0f;
            string customerNameForDialogue = "";

            switch (customerTypeToSpawn)
            {
                case CustomerType.OfficeWorker:
                    spritePrefix = _officeWorkerSpritePrefixes.Any() ? _officeWorkerSpritePrefixes[_random.Next(_officeWorkerSpritePrefixes.Count)] : "cust1";
                    patienceModifier = 0.8f;
                    customerNameForDialogue = $"OfficeWorker ({spritePrefix})";
                    break;

                case CustomerType.Tourist:
                    spritePrefix = _touristSpritePrefixes.Any() ? _touristSpritePrefixes[_random.Next(_touristSpritePrefixes.Count)] : "cust4";
                    patienceModifier = 1.2f;
                    customerNameForDialogue = $"Tourist ({spritePrefix})";
                    break;

                case CustomerType.Student:
                    spritePrefix = _studentSpritePrefixes.Any() ? _studentSpritePrefixes[_random.Next(_studentSpritePrefixes.Count)] : "cust2";
                    patienceModifier = 1.0f;
                    customerNameForDialogue = $"Student ({spritePrefix})";
                    break;

                default: 
                    Console.WriteLine($"Warning: SpawnCustomer called with unhandled type: {customerTypeToSpawn}. Defaulting to OfficeWorker.");
                    spritePrefix = _officeWorkerSpritePrefixes.Any() ? _officeWorkerSpritePrefixes[0] : "cust1";
                    patienceModifier = 0.8f;
                    customerNameForDialogue = $"OfficeWorker ({spritePrefix})";
                    customerTypeToSpawn = CustomerType.OfficeWorker; 
                    break;
            }

            if (_genericOrderRequests.Any())
            {
                dialogueSet.AddLine(customerNameForDialogue, _genericOrderRequests[_random.Next(_genericOrderRequests.Count)]);
            }
            else 
            {
                dialogueSet.AddLine(customerNameForDialogue, "Can I get a drink?");
            }

            NormalCustomer newCustomer = new NormalCustomer(newCustomerId, customerTypeToSpawn, spritePrefix, dialogueSet, patienceModifier);

            string imagesBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            newCustomer.LoadCharacterTextures(imagesBasePath, spritePrefix);

            System.Console.WriteLine($"Spawning {newCustomer.CustomerID} ({customerTypeToSpawn}), SpritePrefix: {spritePrefix}. PatienceMod: {patienceModifier}. Initial Dialogue: '{dialogueSet.Texts.FirstOrDefault()}'");
            return newCustomer;
        }

        /// <summary>
        /// Manages the spawning of new customers into the shop over time.
        /// Considers the current level's allowed customer types, queue capacity, and spawn cooldowns.
        /// Can spawn both normal and supernatural customers based on level rules and random chance.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last game update, in seconds.</param>
        /// <param name="currentLevel">The current <see cref="Level"/> object, providing rules for spawning. Can be null, in which case no spawning occurs.</param>
        /// <param name="gameManager">A reference to the <see cref="GameManager"/> for accessing game state and triggering events.</param>
        public void UpdateCustomerSpawning(float deltaTime, Level? currentLevel, GameManager gameManager)
        {
            if (currentLevel == null) return;

            if (_spawnCooldownTimer > 0)
            {
                _spawnCooldownTimer -= deltaTime;
                if (_spawnCooldownTimer <= 0)
                {
                    _spawnCooldownTimer = 0;
                    Console.WriteLine("ShopLayout: Spawn cooldown finished.");
                    _customerSpawnTimer = 0f; 
                    _nextSpawnDelay = GetRandomSpawnDelay(0.5f, 1.5f); 
                }
                else
                {
                    return; 
                }
            }

            if (_customerQueue.Count >= _maxQueueSize)
            {
                _customerSpawnTimer = 0f;
                return;
            }

            _customerSpawnTimer += deltaTime;

            if (_customerSpawnTimer >= _nextSpawnDelay)
            {
                _customerSpawnTimer = 0f;
                Customer? newCust = null; 

                bool trySpawnSupernatural = false;
                if (currentLevel.AllowedSupernaturalTypes.Any())
                {
                    if (_random.Next(0, 2) == 0 || !currentLevel.UnlockedIngredients.Any(ing => !ing.IsSupernatural))
                    {
                        trySpawnSupernatural = true;
                    }
                }

                if (trySpawnSupernatural)
                {
                    var spawnableSCTypes = currentLevel.AllowedSupernaturalTypes.ToList();
                    
                    if (_lastSpawnedSupernaturalType.HasValue && spawnableSCTypes.Count > 1) 
                    {
                        spawnableSCTypes.Remove(_lastSpawnedSupernaturalType.Value);
                        if (!spawnableSCTypes.Any()) 
                        {
                            spawnableSCTypes.Add(_lastSpawnedSupernaturalType.Value); 
                        }
                    }
                    SupernaturalCustomerType scTypeToSpawn = spawnableSCTypes[_random.Next(spawnableSCTypes.Count)];

                    bool canSpawnThisSupernatural = true; 

                    if (scTypeToSpawn == SupernaturalCustomerType.Toyol && gameManager.IsIngredientCurrentlyStolen)
                    {
                        Console.WriteLine("ShopLayout: Skipping Toyol spawn because an ingredient is already stolen.");
                        canSpawnThisSupernatural = false;
                    }
                    
                    if (canSpawnThisSupernatural)
                    {
                        string spritePrefixForSC = "";
                        CustomerMood mood = (CustomerMood)_random.Next(Enum.GetValues(typeof(CustomerMood)).Length);

                    switch (scTypeToSpawn)
                    {
                        case SupernaturalCustomerType.Toyol: spritePrefixForSC = "toyol"; break;
                        case SupernaturalCustomerType.FireMonster: spritePrefixForSC = "FireMonster"; break; 
                        case SupernaturalCustomerType.Ghost: spritePrefixForSC = "ghost"; break; 
                        case SupernaturalCustomerType.Alien: spritePrefixForSC = "alien"; break; 
                    }

                    Dialogue scDialogue = new Dialogue();
                    switch (scTypeToSpawn)
                    {
                        case SupernaturalCustomerType.Toyol:
                            spritePrefixForSC = "toyol";
                            scDialogue.AddLine("Toyol", "This looks interesting, hehe. You would't mind if I take it, wouldn't you?");
                            break;

                        case SupernaturalCustomerType.FireMonster: 
                            spritePrefixForSC = "FireMonster"; 
                            scDialogue.AddLine("Fire Monster", "Give me the HOTTEST, SPICIEST drink you've got!");
                            break; 

                        case SupernaturalCustomerType.Ghost: 
                            spritePrefixForSC = "ghost";
                            scDialogue.AddLine("Ghost", "Give me a drink that is as dark and bitter as my soul...");
                            break; 

                        case SupernaturalCustomerType.Alien: 
                            spritePrefixForSC = "alien"; 
                            scDialogue.AddLine("Alien", "My analysis requires a beverage with a precise chemical profile...");
                            break; 
                    }

                    newCust = new SupernaturalCustomer(
                        System.Guid.NewGuid().ToString().Substring(0, 8),
                        spritePrefixForSC,
                        scDialogue, 
                        scTypeToSpawn,
                        mood
                    );
                        Console.WriteLine($"ShopLayout: Attempting to spawn SUPERNATURAL: {scTypeToSpawn} with prefix '{spritePrefixForSC}' and ID {newCust.CustomerID}");
                    }
                }

                if (newCust == null)
                {
                    CustomerType[] availableNormalTypes = (CustomerType[])Enum.GetValues(typeof(CustomerType));
                    if (availableNormalTypes.Length > 0)
                    {
                        CustomerType typeToSpawn = availableNormalTypes[_random.Next(availableNormalTypes.Length)];
                        newCust = SpawnCustomer(typeToSpawn, gameManager); 
                        Console.WriteLine($"ShopLayout: Attempting to spawn NORMAL: {typeToSpawn} (ID: {newCust?.CustomerID})");
                    }
                    else
                    {
                        Console.WriteLine("ShopLayout: No normal customer types available to spawn.");
                    }
                }

                if (newCust is SupernaturalCustomer scCust && scCust != null)
                {
                    string imagesBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    scCust.LoadCharacterTextures(imagesBasePath, scCust.SpritePrefix);

                    if (scCust.Type == SupernaturalCustomerType.FireMonster)
                    {
                        Console.WriteLine($"ShopLayout: FireMonster {scCust.CustomerID} spawned, triggering station overheat.");
                        gameManager.SetBrewingStationOverheated(true, true); 
                    }
                }
                
                if (newCust != null)
                {
                    AddCustomerToQueue(newCust);
                    if (newCust is SupernaturalCustomer sc) _lastSpawnedSupernaturalType = sc.Type;
                    else _lastSpawnedSupernaturalType = null;
                }
                _nextSpawnDelay = GetRandomSpawnDelay();
            }
        }

        /// <summary>
        /// Adds a new station to the shop's layout if it's not null and not already present.
        /// </summary>
        /// <param name="station">The <see cref="Station"/> object to add.</param>
        public void AddStation(Station station)
        {
            if (station != null && !_stations.Contains(station))
            {
                _stations.Add(station);
                System.Console.WriteLine($"Station '{station.StationID}' added to ShopLayout at ({station.Position.X}, {station.Position.Y}). Holds: {station.AssignedEquipment?.EquipmentName ?? "Nothing"}");
            }
        }

        /// <summary>
        /// Called when a customer leaves a queue slot, potentially initiating a cooldown before that slot can be refilled.
        /// This helps manage the flow of new customers.
        /// </summary>
        public void NotifySlotOpened()
        {
            if (_spawnCooldownTimer <= 0) 
            {
                _spawnCooldownTimer = SPAWN_COOLDOWN_ON_LEAVE;
                _customerSpawnTimer = 0f; 
                //Console.WriteLine($"ShopLayout: Slot opened, spawn cooldown of {SPAWN_COOLDOWN_ON_LEAVE}s initiated.");
            }
        }

        public void RemoveCustomerFromQueue(Customer customer)
        {
            if (_customerQueue.Contains(customer))
            {
                var list = _customerQueue.ToList();
                list.Remove(customer);
                _customerQueue.Clear();
                foreach(var c in list)
                {
                    _customerQueue.Enqueue(c);
                }
            }
        }
    }
}
