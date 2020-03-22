# discio

Discio allows you to store and manage data using a local JSON file. Discio is not intended to replace a NoSQL database but it can be used for prototyping or when only small amounts of data need to be managed (ex. Settings, roaming profiles, etc.).

## Usage

Creating a repository
```c#
   const string dataFolder = @"C:\Temp\data";
           
   var dataSrc = new Source(dataFolder);

   //If the repository "users" does not exists create it
   if (!dataSrc.Exists("users")) {}
         dataSrc.Create("users");
   }
```

All data models must implement the interface "IDiscio":
```c#
    public class User : IDiscio
    {
        public string ID { set; get; }
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public int Age { set; get; }
    }
```
 

### The class **Manager** gives you access to a repository. 

First, you need to create an instance of **Manager** by supplying 
the data Model as a type parameter and you can either pass the repository name and a data source as constructor parameters or 
assign the source to **SiteSources.Sources** (the first entry will be used as the default source) 
and just pass the repository name.

Passing the source folder to the constructor:
```c#
    var dataFolder = new Source(@"C:\Temp\data");

    var man = new Manager<User>("users", dataFolder);
```

Assign the source to SiteSources:
```c#
//You can do this when the application loads and 
//it will be available to the Manager throughout the application.
 SiteSources.Sources["main"] = new Source(@"C:\Temp\data");  
             
 var man = new Manager<User>("users");
```

Let's add two users:
```c#
  var man = new Manager<User>("users");

  man.Insert(new User
  {
      FirstName = "Joe",
      LastName = "Flacco",
      Age = 29
  });

  man.Insert(new User
  {
      FirstName = "Leo",
      LastName = "Messi",
      Age = 28
  });

  man.Commit();
```

Let's select all the records
```c#
var users = man.Select();

  foreach (var u in users)
  {
       Console.WriteLine("{0} {1}",u.FirstName,u.LastName);
  }
```

Using a where predicate
```c#
 var users = man.Select(m => m.LastName == "Messi");

 foreach (var u in users)
  {
      Console.WriteLine("{0} {1}",u.FirstName,u.LastName);
  }
```

Deleting a user
```c#
 man.Delete(m => m.LastName == "Messi");
 man.Commit();
```

Updating a user
```c#
  var joe = man.First(m => m.LastName == "Flacco");

  joe.Age = 28;

  man.Update(joe);
  man.Commit();
```


