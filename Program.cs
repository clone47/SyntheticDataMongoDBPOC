using MongoDB.Bson;
using MongoDB.Driver;
using Bogus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string connectionString = "<mongodb+srv>";
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase("POC_DB");
        var collection = database.GetCollection<BsonDocument>("POC_Data");

        // Clear collection if exists
        await collection.DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);

        // Faker setup
        var faker = new Faker("en");
        int batchSize = 10000;
        int totalDocuments = 900000;

        Console.WriteLine($"Generating and inserting {totalDocuments} documents...");
        for (int i = 0; i < totalDocuments / batchSize; i++)
        {
            var documents = new List<BsonDocument>();
            for (int j = 0; j < batchSize; j++)
            {
                var createDate = faker.Date.Past(2); // Past 2 years
                var document = new BsonDocument
                {
                    { "_id", ObjectId.GenerateNewId() },
                    { "CreatedBy", faker.Random.Guid().ToString() },
                    { "CreateDate", createDate },
                    { "LastUpdateDate", createDate.AddMinutes(faker.Random.Int(1, 1000)) },
                    { "ProcessState", faker.PickRandom(new[] { "INITIATED", "COMPLETED", "IN_PROGRESS", "FAILED" }) },
                    { "Tags", new BsonArray(faker.Random.WordsArray(2)) },
                    { "ProcessName", faker.Commerce.ProductName() },
                    { "ProcessVersion", faker.Random.Float(1.0f, 10.0f).ToString("F1") },
                    { "EndCustomer", new BsonDocument
                        {
                            { "FirstName", faker.Name.FirstName() },
                            { "LastName", faker.Name.LastName() },
                            { "Email", faker.Internet.Email() }
                        }
                    },
                    { "Broker", new BsonDocument
                        {
                            { "FirstName", faker.Name.FirstName() },
                            { "LastName", faker.Name.LastName() },
                            { "Email", faker.Internet.Email() }
                        }
                    }
                };
                documents.Add(document);
            }

            await collection.InsertManyAsync(documents);
            Console.WriteLine($"Inserted batch {i + 1}/{totalDocuments / batchSize}");
        }

        Console.WriteLine("Data generation completed!");
    }
}
