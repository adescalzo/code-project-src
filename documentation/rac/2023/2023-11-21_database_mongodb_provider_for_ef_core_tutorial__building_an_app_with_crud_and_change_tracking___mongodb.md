```yaml
---
title: "MongoDB Provider for EF Core Tutorial: Building an App with CRUD and Change Tracking | MongoDB"
source: https://www.mongodb.com/developer/languages/csharp/crud-changetracking-mongodb-provider-for-efcore/?utm_campaign=devrel&utm_source=third-party-content&utm_medium=cta&utm_content=ef-core-9-support&utm_term=luce.carter&utm_campaign=devrel&utm_source=third-party-content&utm_medium=cta&utm_content=ef-core-9-support&utm_term=luce.carter
date_published: 2023-11-21T17:00:00.000Z
date_captured: 2025-08-12T14:21:49.294Z
domain: www.mongodb.com
author: Luce Carter
category: database
technologies: [MongoDB, Entity Framework Core, MongoDB Provider for Entity Framework Core, .NET, ASP.NET Core MVC, NuGet, Visual Studio, GitHub, MongoDB Atlas, Bootstrap]
programming_languages: [C#]
tags: [mongodb, ef-core, dotnet, orm, crud, data-access, web-application, tutorial, csharp, change-tracking]
key_concepts: [Object Relational Mapper, CRUD operations, Change Tracking, Repository Pattern, Dependency Injection, Model-View-Controller, NoSQL database, Data Modeling]
code_examples: false
difficulty_level: intermediate
summary: |
  [This tutorial guides developers through building a car booking application using ASP.NET Core MVC and the new MongoDB Provider for Entity Framework Core. It demonstrates how to perform Create, Read, Update, and Delete (CRUD) operations on MongoDB documents using EF Core's familiar syntax. A key focus is on EF Core's change tracking capabilities, which efficiently manage updates to data entities. The article covers setting up the project, defining models, configuring EF Core with MongoDB, implementing services, and creating the front-end views, providing a comprehensive example of integrating a NoSQL database with a popular .NET ORM.]
---
```

# MongoDB Provider for EF Core Tutorial: Building an App with CRUD and Change Tracking | MongoDB

![Image of various development tools and concepts, including a leaf, a coffee mug, gears, headphones, and a notebook with a pen, all rendered in a stylized, isometric purple and green color scheme.](https://images.contentstack.io/v3/assets/blt39790b633ee0d5a7/blt94d1ebdcbef3227c/647a3153ced0472c37a2254b/Luce_Profile_Pic.jpg)

# MongoDB Provider for EF Core Tutorial: Building an App with CRUD and Change Tracking

Luce Carter • 18 min read • Published Nov 21, 2023 • Updated Jan 24, 2024

Entity Framework (EF) has been part of .NET for a long time (since .NET 3.51) and is a popular object relational mapper (ORM) for many applications. EF has evolved into EF Core alongside the evolution of .NET. EF Core supports a number of different database providers and can now be used with MongoDB with the help of the [MongoDB Provider for Entity Framework Core](https://mdb.link/efcore-mongodb).

In this tutorial, we will look at how you can build a car booking application using the new [MongoDB Provider for EF Core](https://mdb.link/efcore-nuget) that will support create, read, update, and delete operations (CRUD) as well as change tracking, which helps to automatically update the database and only the fields that have changed.

A car booking system is a good example to explore the benefits of using EF Core with MongoDB because there is a need to represent a diverse range of entities. There will be entities like cars with their associated availability status and location, and bookings including the associated car.

As the system evolves and grows, ensuring data consistency can become challenging. Additionally, as users interact with the system, partial updates to data entities — like booking details or car specifications — will happen more and more frequently. Capturing and efficiently handling these updates is paramount for good system performance and data integrity.

## Prerequisites

In order to follow along with this tutorial, you are going to need a few things:

*   .NET 7.0.
*   Basic knowledge of ASP.NET MVC and C#.
*   Free [MongoDB Atlas account and free tier cluster](https://mdb.link/getting-started-with-atlas-account).

If you just want to see example code, you can view the full code in the [GitHub repository](https://mdb.link/efcore-sample-repo).

## Create the project

ASP.NET Core is a very flexible web framework, allowing you to scaffold out different types of web applications that have slight differences in terms of their UI or structure. For this tutorial, we are going to create an MVC project that will make use of static files and controllers. There are other types of front end you could use, such as React, but MVC with .cshtml views is the most commonly used. To create the project, we are going to use the .NET CLI:

```
dotnet new mvc -o SuperCarBookingSystem
```

Because we used the CLI, although easier, it only creates the csproj file and not the solution file which allows us to open it in Visual Studio, so we will fix that.

```
cd SuperCarBookingSystem
dotnet new sln
dotnet sln .\SuperCarBookingSystem.sln add .\SuperCarBookingSystem.csproj
```

## Add the NuGet packages

Now that we have the new project created, we will want to go ahead and add the required NuGet packages. Either using the NuGet Package Manager or using the .NET CLI commands below, add the MongoDB MongoDB.EntityFrameworkCore and Microsoft.EntityFrameworkCore packages.

```
dotnet add package MongoDB.EntityFrameworkCore --version 7.0.0-preview.1
dotnet add package Microsoft.EntityFrameworkCore
```

At the time of writing, the MongoDB.EntityFrameworkCore is in preview, so if using the NuGet Package Manager UI inside Visual Studio, be sure to tick the “include pre-release” box or you won’t get any results when searching for it.

## Create the models

Before we can start implementing the new packages we just added, we need to create the models that represent the entities we want in our car booking system that will of course be stored in MongoDB Atlas as documents. In the following subsections, we will create the following models:

*   Car
*   Booking
*   MongoDBSettings

### Car

First, we need to create our car model that will represent the cars that are available to be booked in our system.

1.  Create a new class in the Models folder called Car.
2.  Add the following code:

```csharp
using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace SuperCarBookingSystem.Models
{
    [Collection("cars")]    
    public class Car
    {
       
        public ObjectId Id { get; set; }
       
        [Required(ErrorMessage = "You must provide the make and model")]
        [Display(Name = "Make and Model")]
        public string? Model { get; set; }


      
        [Required(ErrorMessage = "The number plate is required to identify the vehicle")]
        [Display(Name = "Number Plate")]
        public string NumberPlate { get; set; }


        [Required(ErrorMessage = "You must add the location of the car")]
        public string? Location { get; set; }


        public bool IsBooked { get; set; } = false;
    }
}
```

The collection attribute before the class tells the application what collection inside the database we are using. This allows us to have differing names or capitalization between our class and our collection should we want to.

### Booking

We also need to create a booking class to represent any bookings we take in our system.

1.  Create a new class inside the Models folder called Booking.
2.  Add the following code to it:

```csharp
 using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace SuperCarBookingSystem.Models
{
    [Collection("bookings")]
    public class Booking
    {
        public ObjectId Id { get; set; }


        public ObjectId CarId { get; set; }


        public string CarModel { get; set; }


        [Required(ErrorMessage = "The start date is required to make this booking")]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }


        [Required(ErrorMessage = "The end date is required to make this booking")]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
    }
}
```

### MongoDBSettings

Although it won’t be a document in our database, we need a model class to store our MongoDB-related settings so they can be used across the application.

1.  Create another class in Models called MongoDBSettings.
2.  Add the following code:

```csharp
public class MongoDBSettings
{
    public string AtlasURI { get; set; }
    public string DatabaseName { get; set; }
}
```

## Setting up EF Core

This is the exciting part. We are going to start to implement EF Core and take advantage of the new MongoDB Provider. If you are used to working with EF Core already, some of this will be familiar to you.

### CarBookingDbContext

1.  In a location of your choice, create a class called CarBookingDbContext. I placed it inside a new folder called Services.
2.  Replace the code inside the namespace with the following:

```csharp
using Microsoft.EntityFrameworkCore;
using SuperCarBookingSystem.Models;

namespace SuperCarBookingSystem.Services
{
    public class CarBookingDbContext : DbContext
    {
        public DbSet<Car> Cars { get; init; }      


        public DbSet<Booking> Bookings { get; init; }


        public CarBookingDbContext(DbContextOptions options)
        : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Car>();
            modelBuilder.Entity<Booking>();
        }
    }
}
```

If you are used to EF Core, this will look familiar. The class extends the DbContext and we create DbSet properties that store the models that will also be present in the database. We also override the OnModelCreating method. You may notice that unlike when using SQL Server, we don’t call .ToTable(). We could call ToCollection instead but this isn’t required here as we specify the collection using attributes on the classes.

### Add connection string and database details to appsettings

Earlier, we created a MongoDBSettings model, and now we need to add the values that the properties map to into our appsettings.

1.  In both appsettings.json and appsettings.Development.json, add the following new section:

    ```json
     "MongoDBSettings": {
       "AtlasURI": "mongodb+srv://<username>:<password>@<url>",
       "DatabaseName": "cargarage"
     }
    ```

2.  Replace the Atlas URI with your own [connection string](https://mdb.link/gettingconnectionstring) from Atlas.

### Updating program.cs

Now we have configured our models and DbContext, it is time to add them to our program.cs file.

After the existing line `builder.Services.AddControllersWithViews();`, add the following code:

```csharp
var mongoDBSettings = builder.Configuration.GetSection("MongoDBSettings").Get<MongoDBSettings>();
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));

builder.Services.AddDbContext<CarBookingDbContext>(options =>
options.UseMongoDB(mongoDBSettings.AtlasURI ?? "", mongoDBSettings.DatabaseName ?? ""));
```

## Creating the services

Now, it is time to add the services we will use to talk to the database via the CarBookingDbContext we created. For each service, we will create an interface and the class that implements it.

### ICarService and CarService

The first interface and service we will implement is for carrying out the CRUD operations on the cars collection. This is known as the repository pattern. You may see people interact with the DbContext directly. But most people use this pattern, which is why we are including it here.

1.  If you haven’t already, create a Services folder to store our new classes.
2.  Create an ICarService interface and add the following code for the methods we will implement:

```csharp
using MongoDB.Bson;
using SuperCarBookingSystem.Models;

namespace SuperCarBookingSystem.Services
{
    public interface ICarService
    {
        IEnumerable<Car> GetAllCars();
        Car? GetCarById(ObjectId id);

        void AddCar(Car newCar);

        void EditCar(Car updatedCar);

        void DeleteCar(Car carToDelete);
    }
}
```

3.  Create a CarService class file.
4.  Update the CarService class declaration so it implements the ICarService we just created:

```csharp
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using SuperCarBookingSystem.Models;

namespace SuperCarBookingSystem.Services
{
    public class CarService : ICarService
	{
```

5.  This will cause a red squiggle to appear underneath ICarService as we haven’t implemented all the methods yet, but we will implement the methods one by one.
6.  Add the following code after the class declaration that adds a local CarBookingDbContext object and a constructor that gets an instance of the DbContext via dependency injection.

```csharp
 private readonly CarBookingDbContext _carDbContext;
 public CarService(CarBookingDbContext carDbContext)
 {
     _carDbContext = carDbContext;
 }
```

7.  Next, we will implement the GetAllCars method so add the following code:

```csharp
public IEnumerable<Car> GetAllCars()
{
       return _carDbContext.Cars.OrderBy(c => c.Id).AsNoTracking().AsEnumerable<Car>();
}
```

The id property here maps to the \_id field in our document which is a special MongoDB ObjectId type and is auto-generated when a new document is created. But what is useful about the \_id property is that it can actually be used to order documents because of how it is generated under the hood.

If you haven’t seen it before, the `AsNoTracking()` method is part of EF Core and prevents EF tracking changes you make to an object. This is useful for reads when you know no changes are going to occur.

8.  Next, we will implement the method to get a specific car using its Id property:

```csharp
public Car? GetCarById(ObjectId id)
{
    return _carDbContext.Cars.FirstOrDefault(c  => c.Id == id);
}
```

Then, we will add the AddCar implementation:

```csharp
public void AddCar(Car car)
{
    _carDbContext.Cars.Add(car);

    _carDbContext.ChangeTracker.DetectChanges();
    Console.WriteLine(_carDbContext.ChangeTracker.DebugView.LongView);

    _carDbContext.SaveChanges();
}
```

In a production environment, you might want to use something like ILogger to track these changes rather than printing to the console. But this will allow us to clearly see that a new entity has been added, showing change tracking in action.

9.  EditCar is next:

```csharp
public void EditCar(Car car)
{
      var carToUpdate = _carDbContext.Cars.FirstOrDefault(c => c.Id == car.Id);

    if(carToUpdate != null)
    {                
        carToUpdate.Model = car.Model;
        carToUpdate.NumberPlate = car.NumberPlate;
        carToUpdate.Location = car.Location;
        carToUpdate.IsBooked = car.IsBooked;

        _carDbContext.Cars.Update(carToUpdate);

        _carDbContext.ChangeTracker.DetectChanges();
        Console.WriteLine(_carDbContext.ChangeTracker.DebugView.LongView);

        _carDbContext.SaveChanges();
            
    }
  else
    {
        throw new ArgumentException("The car to update cannot be found. ");
    }
}
```

Again, we add a call to print out information from change tracking as it will show that the new EF Core Provider, even when using MongoDB as the database, is able to track modifications.

10. Finally, we need to implement DeleteCar:

```csharp
public void DeleteCar(Car car)
{
	var carToDelete = _carDbContext.Cars.Where(c => c.Id == car.Id).FirstOrDefault();

	if(carToDelete != null) {
       _carDbContext.Cars.Remove(carToDelete);
	   _carDbContext.ChangeTracker.DetectChanges();
       Console.WriteLine(_carDbContext.ChangeTracker.DebugView.LongView);
      _carDbContext.SaveChanges();
      }
    else {
        throw new ArgumentException("The car to delete cannot be found.");
    }
}
```

### IBookingService and BookingService

Next up is our IBookingService and BookingService.

1.  Create the IBookingService interface and add the following methods:

```csharp
using MongoDB.Bson;
using SuperCarBookingSystem.Models;
namespace SuperCarBookingSystem.Services
{
    public interface IBookingService
    {
        IEnumerable<Booking> GetAllBookings();
        Booking? GetBookingById(ObjectId id);

        void AddBooking(Booking newBooking);

        void EditBooking(Booking updatedBooking);

        void DeleteBooking(Booking bookingToDelete);
    }
}
```

2.  Create the BookingService class, and replace your class with the following code that implements all the methods:

```csharp
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using SuperCarBookingSystem.Models;

namespace SuperCarBookingSystem.Services
{
    public class BookingService : IBookingService
    {
        private readonly CarBookingDbContext _carDbContext;

        public BookingService(CarBookingDbContext carDBContext)
        {
            _carDbContext = carDBContext;
        }
        public void AddBooking(Booking newBooking)
        {
            var bookedCar = _carDbContext.Cars.FirstOrDefault(c => c.Id == newBooking.CarId);
            if (bookedCar == null)
            {
                throw new ArgumentException("The car to be booked cannot be found.");
            }

            newBooking.CarModel = bookedCar.Model;

            bookedCar.IsBooked = true;
            _carDbContext.Cars.Update(bookedCar);

            _carDbContext.Bookings.Add(newBooking);

            _carDbContext.ChangeTracker.DetectChanges();
            Console.WriteLine(_carDbContext.ChangeTracker.DebugView.LongView);

            _carDbContext.SaveChanges();
        }

        public void DeleteBooking(Booking booking)
        {
            var bookedCar = _carDbContext.Cars.FirstOrDefault(c => c.Id == booking.CarId);
            bookedCar.IsBooked = false;

            var bookingToDelete = _carDbContext.Bookings.FirstOrDefault(b => b.Id == booking.Id);

            if(bookingToDelete != null)
            {
                _carDbContext.Bookings.Remove(bookingToDelete);
                _carDbContext.Cars.Update(bookedCar);

                _carDbContext.ChangeTracker.DetectChanges();
                Console.WriteLine(_carDbContext.ChangeTracker.DebugView.LongView);

                _carDbContext.SaveChanges();
            }
            else
            {
                throw new ArgumentException("The booking to delete cannot be found.");
            }
        }

        public void EditBooking(Booking updatedBooking)
        {
           var bookingToUpdate = _carDbContext.Bookings.FirstOrDefault(b => b.Id == updatedBooking.Id);
           
            
            if (bookingToUpdate != null)
            {               
                bookingToUpdate.StartDate = updatedBooking.StartDate;
                bookingToUpdate.EndDate = updatedBooking.EndDate;
                
                _carDbContext.Bookings.Update(bookingToUpdate);

                _carDbContext.ChangeTracker.DetectChanges();
                _carDbContext.SaveChanges();

                Console.WriteLine(_carDbContext.ChangeTracker.DebugView.LongView);
            }  
            else 
            { 
                throw new ArgumentException("Booking to be updated cannot be found");
            }
            
        }

        public IEnumerable<Booking> GetAllBookings()
        {
            return _carDbContext.Bookings.OrderBy(b => b.StartDate).AsNoTracking().AsEnumerable<Booking>();
        }

        public Booking? GetBookingById(ObjectId id)
        {
            return _carDbContext.Bookings.AsNoTracking().FirstOrDefault(b => b.Id == id);
        }
        
    }
}
```

This code is very similar to the code for the CarService class but for bookings instead.

### Adding them to Dependency Injection

The final step for the services is to add them to the dependency injection container.

Inside Program.cs, add the following code after the code we added there earlier:

```csharp
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IBookingService, BookingService>();
```

## Creating the view models

Before we implement the front end, we need to add the view models that will act as a messenger between our front and back ends where required. Even though our application is quite simple, implementing the view model is still good practice as it helps decouple the pieces of the app.

### CarListViewModel

The first one we will add is the CarListViewModel. This will be used as the model in our Razor page later on for listing cars in our database.

1.  Create a new folder in the root of the project called ViewModels.
2.  Add a new class called CarListViewModel.
3.  Add `public IEnumerable<Car> Cars { get; set; }` inside your class.

### CarAddViewModel

We also want a view model that can be used by the Add view we will add later.

1.  Inside the ViewModels folder, create a new class called CarAddViewModel.
2.  Add `public Car? Car { get; set; }`.

### BookingListViewModel

Now, we want to do something very similar for bookings, starting with BookingListViewModel.

1.  Create a new class in the ViewModels folder called BookingListViewModel.
2.  Add `public IEnumerable<Booking> Bookings { get; set; }`.

### BookingAddViewModel

Finally, we have our BookingAddViewModel.

Create the class and add the property `public Booking? Booking { get; set; }` inside the class.

### Adding to _ViewImports

Later on, we will be adding references to our models and viewmodels in the views. In order for the application to know what they are, we need to add references to them in the \_ViewImports.cshtml file inside the Views folder.

There will already be some references in there, including TagHelpers, so we want to add references to our .Models and .ViewModels folders. When added, it will look something like below, just with your application name instead.

```csharp
@using <YourApplicationName>
@using <YourApplicationName>.Models
@using <YourApplicationName>.ViewModels
```

## Creating the controllers

Now we have the backend implementation and the view models we will refer to, we can start working toward the front end. We will be creating two controllers: one for Car and one for Booking.

### CarController

The first controller we will add is for the car.

1.  Inside the existing Controllers folder, add a new controller. If using Visual Studio, use the MVC Controller - Empty controller template.
2.  Add a local ICarService object and a constructor that fetches it from dependency injection:

```csharp
private readonly ICarService _carService;


public CarController(ICarService carService)
{
    _carService = carService;
}
```

3.  Depending on what your scaffolded controller came with, either create or update the Index function with the following:

```csharp
public IActionResult Index()
{
    CarListViewModel viewModel = new()
    {
        Cars = _carService.GetAllCars(),
    };
    return View(viewModel);
}
```

For the other CRUD operations — so create, update, and delete — we will have two methods for each: one is for Get and the other is for Post.

4.  The HttpGet for Add will be very simple as it doesn’t need to pass any data around:

```csharp
public IActionResult Add()
{
    return View();
}
```

5.  Next, add the Add method that will be called when a new car is requested to be added:

```csharp
 [HttpPost]
 public IActionResult Add(CarAddViewModel carAddViewModel)
 {
     if(ModelState.IsValid)
     {
         Car newCar = new()
         {
             Model = carAddViewModel.Car.Model,
             Location = carAddViewModel.Car.Location,
             NumberPlate = carAddViewModel.Car.NumberPlate
         };


         _carService.AddCar(newCar);
         return RedirectToAction("Index");
     }


     return View(carAddViewModel);         
 }
```

6.  Now, we will add the code for editing a car:

```csharp
 public IActionResult Edit(string id)
 {
     if(id == null)
     {
         return NotFound();
     }


     var selectedCar = _carService.GetCarById(new ObjectId(id));
     return View(selectedCar);
 }


 [HttpPost]
 public IActionResult Edit(Car car)
 {
     try
     {
         if(ModelState.IsValid)
         {
             _carService.EditCar(car);
             return RedirectToAction("Index");
         }
         else
         {
             return BadRequest();
         }
     }
     catch (Exception ex)
     {
         ModelState.AddModelError("", $"Updating the car failed, please try again! Error: {ex.Message}");
     }


     return View(car);
 }
```

7.  Finally, we have Delete:

```csharp
public IActionResult Delete(string id) {
    if (id == null)
    {
        return NotFound();
    }


    var selectedCar = _carService.GetCarById(new ObjectId(id));
    return View(selectedCar);
}


[HttpPost]
public IActionResult Delete(Car car)
{
    if (car.Id == null)
    {
        ViewData["ErrorMessage"] = "Deleting the car failed, invalid ID!";
        return View();
    }


    try
    {
        _carService.DeleteCar(car);
        TempData["