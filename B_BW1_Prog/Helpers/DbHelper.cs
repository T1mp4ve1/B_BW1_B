using B_BW1_Prog.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace B_BW1_Prog.Helpers
{
    public class DbHelper
    {
        //I NOMI
        public static string DBname = "BW1_B";
        public static string Table_All_Products = "Products";
        public static string Table_All_Imgs = "Imgs";

        //CONNESSIONI
        private const string _masterConnectionString = """
            Server=T1MP4VE1\SQLEXPRESS;
            User Id=sa;
            Password=7210;
            Database=master;
            TrustServerCertificate=True;
            """;
        private static readonly string _newDbConnectionString = $"""
            Server=T1MP4VE1\SQLEXPRESS;
            User Id=sa;
            Password=7210;
            Database={DBname};
            TrustServerCertificate=True;
            """;

        //FUNZIONE DI CREAZIONE DB E TABELLE
        public static void InitializeDb()
        {
            CreateDb(DBname);
            CreateTable(Table_All_Products);
            CreateImagesTable(Table_All_Imgs);
        }

        //CREA DB
        private static void CreateDb(string name)
        {
            using var connection = new SqlConnection(_masterConnectionString);
            connection.Open();

            var commandText = $"""
                IF NOT EXISTS(SELECT name FROM sys.databases WHERE name = '{name}')
                BEGIN
                CREATE DATABASE [{name}];
                END
                """;

            var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.ExecuteNonQuery();
        }

        //PRODUCTS TABELLA
        private static void CreateTable(string name)
        {
            using var connection = new SqlConnection(_newDbConnectionString);
            connection.Open();

            var commandText = $"""
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '{name}')
                BEGIN
                CREATE TABLE {name} (
                idProduct UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
                displayName NVARCHAR(max) NOT NULL,
                descriptionPro NVARCHAR(max) NOT NULL,
                price DECIMAL(10,2) NOT NULL,
                inStock INT NOT NULL
                );
                END
                """;

            var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.ExecuteNonQuery();
        }

        //IMGS TABELLA
        private static void CreateImagesTable(string name)
        {
            using var connection = new SqlConnection(_newDbConnectionString);
            connection.Open();

            var commandText = $"""
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '{name}')
                BEGIN
                CREATE TABLE {name} (
                idImg UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
                idProduct UNIQUEIDENTIFIER NOT NULL,
                URLimg NVARCHAR(max) NOT NULL,
                CONSTRAINT FK_Imgs_Products 
                FOREIGN KEY (idProduct) REFERENCES Products(idProduct)
                );
                END
                """;

            var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.ExecuteNonQuery();
        }

        //RICEVE TUTTI PRODOTTI
        public static List<ProductModel> GetAllProducts()
        {
            using var connection = new SqlConnection(_newDbConnectionString);
            connection.Open();

            var commandText = $"""
                SELECT * FROM {Table_All_Products};
                """;

            var command = connection.CreateCommand();
            command.CommandText = commandText;
            var reader = command.ExecuteReader();

            var allProducts = new List<ProductModel>();

            while (reader.Read())
            {
                allProducts.Add(new ProductModel
                {
                    Id = reader.GetGuid(reader.GetOrdinal("idProduct")),
                    name = reader.GetString(reader.GetOrdinal("displayName")),
                    description = reader.GetString(reader.GetOrdinal("descriptionPro")),
                    price = reader.GetDecimal(reader.GetOrdinal("price")),
                    inStock = reader.GetInt32(reader.GetOrdinal("inStock"))
                });
            }
            return allProducts;
        }

        //AGGIUNGI PRODOTTO
        public static bool AddProduct(ProductModel product)
        {
            bool result = false;
            using var connection = new SqlConnection(_newDbConnectionString);
            connection.Open();

            var commandText = $"""
                INSERT INTO {Table_All_Products}
                (displayName, descriptionPro, price, inStock)
                VALUES (@name, @desc, @price, @inStock);
                """;

            var command = connection.CreateCommand();
            command.CommandText = commandText;

            command.Parameters.Add("@name", SqlDbType.NVarChar, 50).Value = product.name;
            command.Parameters.Add("@desc", SqlDbType.NVarChar, 1000).Value = product.description;
            var p = command.Parameters.Add("@price", SqlDbType.Decimal);
            p.Precision = 10;
            p.Scale = 2;
            p.Value = product.price;
            command.Parameters.Add("@inStock", SqlDbType.Int).Value = product.inStock;



            try
            {
                command.ExecuteNonQuery();
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        //RICEVE IMMAGINI
        public static List<ImgsModel> GetAllImgs()
        {
            using var connection = new SqlConnection(_newDbConnectionString);
            connection.Open();

            var commandText = $"""
                SELECT * FROM {Table_All_Imgs}
                """;

            var command = connection.CreateCommand();
            command.CommandText = commandText;
            var reader = command.ExecuteReader();

            var allImgs = new List<ImgsModel>();

            while (reader.Read())
            {
                allImgs.Add(new ImgsModel
                {
                    IdImage = reader.GetGuid(reader.GetOrdinal("idImg")),
                    IdProduct = reader.GetGuid(reader.GetOrdinal("idProduct")),
                    URLimg = reader.GetString(reader.GetOrdinal("URLimg"))
                });
            }
            return allImgs;
        }

        //AGGIUNGI IMMAGINE
        public static bool AddImg(ImgsModel img, Guid pid)
        {
            bool result = false;

            using var connection = new SqlConnection(_newDbConnectionString);
            connection.Open();

            var commandText = $"""
                INSERT INTO {Table_All_Imgs}
                (idProduct, URLimg)
                VALUES (@IdProduct, @URLimg);
                """;

            var command = connection.CreateCommand();
            command.CommandText = commandText;

            command.Parameters.Add("@IdProduct", SqlDbType.UniqueIdentifier);
            command.Parameters.Add("@URLimg", SqlDbType.NVarChar, -1);

            command.Parameters["@IdProduct"].Value = pid;
            command.Parameters["@URLimg"].Value = img.URLimg;

            try
            {
                command.ExecuteNonQuery();
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }
    }
}