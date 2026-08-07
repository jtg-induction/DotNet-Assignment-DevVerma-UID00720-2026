namespace Assignment3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Addresses",
                c => new
                {
                    Id = c.Long(nullable: false, identity: true),
                    UserId = c.Long(),
                    BuildingNumber = c.String(nullable: false, maxLength: 20),
                    Locality = c.String(nullable: false, maxLength: 100),
                    City = c.String(nullable: false, maxLength: 100),
                    State = c.String(nullable: false, maxLength: 100),
                    Country = c.String(nullable: false, maxLength: 100),
                    PostalCode = c.String(nullable: false, maxLength: 20),
                    CreatedAt = c.DateTime(nullable: false),
                    UpdatedAt = c.DateTime(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);

            CreateTable(
                "dbo.Users",
                c => new
                {
                    Id = c.Long(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 100),
                    Email = c.String(nullable: false, maxLength: 255),
                    Password = c.String(nullable: false, maxLength: 255),
                    Role = c.String(nullable: false, maxLength: 20),
                    IsActive = c.Boolean(nullable: false),
                    Balance = c.Decimal(nullable: false, precision: 18, scale: 2),
                    CreatedAt = c.DateTime(nullable: false),
                    UpdatedAt = c.DateTime(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Orders",
                c => new
                {
                    Id = c.Long(nullable: false, identity: true),
                    UserId = c.Long(nullable: false),
                    RestaurantId = c.Int(nullable: false),
                    Status = c.String(nullable: false, maxLength: 20),
                    TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    BuildingNumber = c.String(nullable: false, maxLength: 20),
                    Locality = c.String(nullable: false, maxLength: 100),
                    City = c.String(nullable: false, maxLength: 100),
                    State = c.String(nullable: false, maxLength: 100),
                    Country = c.String(nullable: false, maxLength: 100),
                    PostalCode = c.String(nullable: false, maxLength: 20),
                    CreatedAt = c.DateTime(nullable: false),
                    UpdatedAt = c.DateTime(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Restaurants", t => t.RestaurantId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.RestaurantId);

            CreateTable(
                "dbo.OrderItems",
                c => new
                {
                    Id = c.Long(nullable: false, identity: true),
                    OrderId = c.Long(nullable: false),
                    MenuItemId = c.Long(nullable: false),
                    Quantity = c.Int(nullable: false),
                    UnitPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                    CreatedAt = c.DateTime(nullable: false),
                    UpdatedAt = c.DateTime(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MenuItems", t => t.MenuItemId)
                .ForeignKey("dbo.Orders", t => t.OrderId)
                .Index(t => t.OrderId)
                .Index(t => t.MenuItemId);

            CreateTable(
                "dbo.MenuItems",
                c => new
                {
                    Id = c.Long(nullable: false, identity: true),
                    RestaurantId = c.Int(nullable: false),
                    Name = c.String(nullable: false, maxLength: 100),
                    Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                    Description = c.String(maxLength: 500),
                    CreatedAt = c.DateTime(nullable: false),
                    UpdatedAt = c.DateTime(),
                    Stock = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Restaurants", t => t.RestaurantId)
                .Index(t => t.RestaurantId);

            CreateTable(
                "dbo.Restaurants",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 100),
                    Email = c.String(nullable: false, maxLength: 255),
                    IsActive = c.Boolean(nullable: false),
                    AddressId = c.Long(nullable: false),
                    CreatedAt = c.DateTime(nullable: false),
                    UpdatedAt = c.DateTime(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Addresses", t => t.AddressId)
                .Index(t => t.AddressId, unique: true, name: "IX_Restaurant_AddressId");

            CreateTable(
                "dbo.RestaurantOwners",
                c => new
                {
                    UserId = c.Long(nullable: false),
                    RestaurantId = c.Int(nullable: false),
                    CreatedAt = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => new { t.UserId, t.RestaurantId })
                .ForeignKey("dbo.Restaurants", t => t.RestaurantId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.RestaurantId);

            CreateTable(
                "dbo.RefreshTokens",
                c => new
                {
                    Id = c.Long(nullable: false, identity: true),
                    UserId = c.Long(nullable: false),
                    Token = c.String(nullable: false, maxLength: 500),
                    ExpiresAt = c.DateTime(nullable: false),
                    CreatedAt = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);


            // DEFAULT CONSTRAINTS

            // User Role = customer
            Sql(@"
            IF NOT EXISTS
            (
                SELECT 1
                FROM sys.default_constraints
                WHERE name = 'DF_Users_Role'
            )
            BEGIN
                ALTER TABLE dbo.Users
                ADD CONSTRAINT DF_Users_Role
                DEFAULT ('customer') FOR Role
            END
            ");

            // User IsActive = true
            Sql(@"
            IF NOT EXISTS
            (
                SELECT 1
                FROM sys.default_constraints
                WHERE name = 'DF_Users_IsActive'
            )
            BEGIN
                ALTER TABLE dbo.Users
                ADD CONSTRAINT DF_Users_IsActive
                DEFAULT (1) FOR IsActive
            END
            ");

            // Restaurant IsActive = true
            Sql(@"
            IF NOT EXISTS
            (
                SELECT 1
                FROM sys.default_constraints
                WHERE name = 'DF_Restaurants_IsActive'
            )
            BEGIN
                ALTER TABLE dbo.Restaurants
                ADD CONSTRAINT DF_Restaurants_IsActive
                DEFAULT (1) FOR IsActive
            END
            ");

            // Order Status = placed
            Sql(@"
            IF NOT EXISTS
            (
                SELECT 1
                FROM sys.default_constraints
                WHERE name = 'DF_Orders_Status'
            )
            BEGIN
                ALTER TABLE dbo.Orders
                ADD CONSTRAINT DF_Orders_Status
                DEFAULT ('placed') FOR Status
            END
            ");



            // CHECK CONSTRAINTS

            // User Role
            Sql(@"
            IF NOT EXISTS
            (
                SELECT 1
                FROM sys.check_constraints
                WHERE name = 'CK_Users_Role'
            )
            BEGIN
                ALTER TABLE dbo.Users
                ADD CONSTRAINT CK_Users_Role
                CHECK
                (
                    Role IN
                    (
                        'customer',
                        'admin',
                        'super_admin'
                    )
                )
            END
            ");

            // Order Status
            Sql(@"
            IF NOT EXISTS
            (
                SELECT 1
                FROM sys.check_constraints
                WHERE name = 'CK_Orders_Status'
            )
            BEGIN
                ALTER TABLE dbo.Orders
                ADD CONSTRAINT CK_Orders_Status
                CHECK
                (
                    Status IN
                    (
                        'placed',
                        'accepted',
                        'rejected',
                        'dispatched',
                        'delivered',
                        'cancelled'
                    )
                )
            END
            ");



            // UNIQUE INDEXES

            // User Email should be unique
            CreateIndex(
                "dbo.Users",
                "Email",
                unique: true,
                name: "IX_Users_Email"
            );

            // Restaurant Email should be unique
            CreateIndex(
                "dbo.Restaurants",
                "Email",
                unique: true,
                name: "IX_Restaurants_Email"
            );
        }


        public override void Down()
        {
            // Drop Unique Indexes
            DropIndex("dbo.Restaurants", "IX_Restaurants_Email");
            DropIndex("dbo.Users", "IX_Users_Email");

            // Drop CHECK Constraints
            Sql("ALTER TABLE dbo.Orders DROP CONSTRAINT CK_Orders_Status");
            Sql("ALTER TABLE dbo.Users DROP CONSTRAINT CK_Users_Role");

            // Drop DEFAULT Constraints
            Sql("ALTER TABLE dbo.Orders DROP CONSTRAINT DF_Orders_Status");
            Sql("ALTER TABLE dbo.Restaurants DROP CONSTRAINT DF_Restaurants_IsActive");
            Sql("ALTER TABLE dbo.Users DROP CONSTRAINT DF_Users_IsActive");
            Sql("ALTER TABLE dbo.Users DROP CONSTRAINT DF_Users_Role");


            DropForeignKey("dbo.Addresses", "UserId", "dbo.Users");
            DropForeignKey("dbo.RefreshTokens", "UserId", "dbo.Users");
            DropForeignKey("dbo.Orders", "UserId", "dbo.Users");
            DropForeignKey("dbo.Orders", "RestaurantId", "dbo.Restaurants");
            DropForeignKey("dbo.OrderItems", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.OrderItems", "MenuItemId", "dbo.MenuItems");
            DropForeignKey("dbo.MenuItems", "RestaurantId", "dbo.Restaurants");
            DropForeignKey("dbo.RestaurantOwners", "UserId", "dbo.Users");
            DropForeignKey("dbo.RestaurantOwners", "RestaurantId", "dbo.Restaurants");
            DropForeignKey("dbo.Restaurants", "AddressId", "dbo.Addresses");
            DropIndex("dbo.RefreshTokens", new[] { "UserId" });
            DropIndex("dbo.RestaurantOwners", new[] { "RestaurantId" });
            DropIndex("dbo.RestaurantOwners", new[] { "UserId" });
            DropIndex("dbo.Restaurants", "IX_Restaurant_AddressId");
            DropIndex("dbo.MenuItems", new[] { "RestaurantId" });
            DropIndex("dbo.OrderItems", new[] { "MenuItemId" });
            DropIndex("dbo.OrderItems", new[] { "OrderId" });
            DropIndex("dbo.Orders", new[] { "RestaurantId" });
            DropIndex("dbo.Orders", new[] { "UserId" });
            DropIndex("dbo.Addresses", new[] { "UserId" });
            DropTable("dbo.RefreshTokens");
            DropTable("dbo.RestaurantOwners");
            DropTable("dbo.Restaurants");
            DropTable("dbo.MenuItems");
            DropTable("dbo.OrderItems");
            DropTable("dbo.Orders");
            DropTable("dbo.Users");
            DropTable("dbo.Addresses");
        }
    }
}
