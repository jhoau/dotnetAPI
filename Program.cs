using Microsoft.Data.SqlClient; 
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Activar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API de Compañías y Empleados", Version = "v1" });
});

var app = builder.Build();

// Usar Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/companias", (string nombre, string direccion) =>
{
    string connString = "Server=.;Database=TareaAPI;User id=SA;Password=Sa123456;TrustServerCertificate=True";

    using (var conn = new SqlConnection(connString))
    {
        conn.Open();
        string query = "INSERT INTO Companias (Nombre, Direccion) VALUES (@nombre, @direccion)";
        
        try
        {
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.Parameters.AddWithValue("@direccion", direccion);

                cmd.ExecuteNonQuery();
                return Results.Created("/companias", new { nombre, direccion });
            }
        }
        catch (Exception ex)
        {
            return Results.BadRequest("Error: " + ex.Message);
        }
    }
});

app.MapGet("/companias", () =>
{
    string connString = "Server=.;Database=TareaAPI;User id=SA;Password=Sa123456;TrustServerCertificate=True";

    var companias = new List<object>();

    using (var conn = new SqlConnection(connString))
    {
        conn.Open();
        string query = "SELECT * FROM Companias";

        try
        {
            using (var cmd = new SqlCommand(query, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    companias.Add(new
                    {
                        Id = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        Direccion = reader.IsDBNull(2) ? null : reader.GetString(2)
                    });
                }

                return Results.Ok(companias);
            }
        }
        catch (Exception ex)
        {
            return Results.Problem("Error: " + ex.Message);
        }
    }
});

app.MapPut("/companias/{id}", (int id, string nombre, string direccion) =>
{
    string connString = "Server=localhost;Database=TareaAPI;User ID=SA;Password=Sa123456;TrustServerCertificate=True;";

    using (var conn = new SqlConnection(connString))
    {
        conn.Open();
        string query = "UPDATE Companias SET Nombre = @nombre, Direccion = @direccion WHERE Id = @id";

        try
        {
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.Parameters.AddWithValue("@direccion", direccion);

                int filasAfectadas = cmd.ExecuteNonQuery();

                if (filasAfectadas == 0)
                {
                    return Results.NotFound($"No se encontró la compañía con ID {id}.");
                }

                return Results.Ok($"Compañía actualizada exitosamente (ID: {id}).");
            }
        }
        catch (Exception ex)
        {
            return Results.BadRequest("Error: " + ex.Message);
        }
    }
});

app.MapDelete("/companias/{id}", (int id) =>
{
    string connString = "Server=localhost;Database=TareaAPI;User ID=SA;Password=Sa123456;TrustServerCertificate=True;";

    using (var conn = new SqlConnection(connString))
    {
        conn.Open();

        try
        {
            // Verificar si tiene empleados
            string checkQuery = "SELECT COUNT(*) FROM Empleados WHERE CompaniaId = @id";
            using (var checkCmd = new SqlCommand(checkQuery, conn))
            {
                checkCmd.Parameters.AddWithValue("@id", id);
                int empleados = (int)checkCmd.ExecuteScalar();

                if (empleados > 0)
                {
                    return Results.BadRequest($"No se puede eliminar. La compañía tiene {empleados} empleados asociados.");
                }
            }

            //Eliminar compañía si no tiene empleados
            string deleteQuery = "DELETE FROM Companias WHERE Id = @id";
            using (var deleteCmd = new SqlCommand(deleteQuery, conn))
            {
                deleteCmd.Parameters.AddWithValue("@id", id);
                int filasAfectadas = deleteCmd.ExecuteNonQuery();

                if (filasAfectadas == 0)
                {
                    return Results.NotFound($"No se encontró la compañía con ID {id}.");
                }

                return Results.Ok($"Compañía con ID {id} eliminada correctamente.");
            }
        }
        catch (Exception ex)
        {
            return Results.Problem("Error: " + ex.Message);
        }
    }
});


/*ENPOINT EMPLEADOS*/

app.MapPost("/empleados", (string nombre, int edad, int companiaId) =>
{
    string connString = "Server=localhost;Database=TareaAPI;User ID=SA;Password=Sa123456;TrustServerCertificate=True;";

    using (var conn = new SqlConnection(connString))
    {
        conn.Open();

        try
        {
            //  Validar que la compañía exista
            string checkCompania = "SELECT COUNT(*) FROM Companias WHERE Id = @companiaId";
            using (var checkCmd = new SqlCommand(checkCompania, conn))
            {
                checkCmd.Parameters.AddWithValue("@companiaId", companiaId);
                int existe = (int)checkCmd.ExecuteScalar();

                if (existe == 0)
                {
                    return Results.BadRequest("La compañía especificada no existe.");
                }
            }

            // Paso 2: Insertar empleado
            string insertQuery = "INSERT INTO Empleados (Nombre, Edad, CompaniaId) VALUES (@nombre, @edad, @companiaId)";
            using (var insertCmd = new SqlCommand(insertQuery, conn))
            {
                insertCmd.Parameters.AddWithValue("@nombre", nombre);
                insertCmd.Parameters.AddWithValue("@edad", edad);
                insertCmd.Parameters.AddWithValue("@companiaId", companiaId);

                insertCmd.ExecuteNonQuery();
                return Results.Created("/empleados", new { nombre, edad, companiaId });
            }
        }
        catch (Exception ex)
        {
            return Results.Problem("Error: " + ex.Message);
        }
    }
});


app.MapGet("/empleados", () =>
{
    string connString = "Server=localhost;Database=TareaAPI;User ID=SA;Password=Sa123456;TrustServerCertificate=True;";

    var empleados = new List<object>();

    using (var conn = new SqlConnection(connString))
    {
        conn.Open();
        string query = @"
            SELECT e.Id, e.Nombre, e.Edad, e.CompaniaId, c.Nombre AS CompaniaNombre
            FROM Empleados e
            INNER JOIN Companias c ON e.CompaniaId = c.Id";

        try
        {
            using (var cmd = new SqlCommand(query, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    empleados.Add(new
                    {
                        Id = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        Edad = reader.GetInt32(2),
                        CompaniaId = reader.GetInt32(3),
                        CompaniaNombre = reader.GetString(4)
                    });
                }

                return Results.Ok(empleados);
            }
        }
        catch (Exception ex)
        {
            return Results.Problem("Error: " + ex.Message);
        }
    }
});

app.MapPut("/empleados/{id}", (int id, string nombre, int edad, int companiaId) =>
{
    string connString = "Server=localhost;Database=TareaAPI;User ID=SA;Password=Sa123456;TrustServerCertificate=True;";

    using (var conn = new SqlConnection(connString))
    {
        conn.Open();

        try
        {
            // Verificar si el empleado existe
            string checkEmpleado = "SELECT COUNT(*) FROM Empleados WHERE Id = @id";
            using (var checkCmd = new SqlCommand(checkEmpleado, conn))
            {
                checkCmd.Parameters.AddWithValue("@id", id);
                int existe = (int)checkCmd.ExecuteScalar();
                if (existe == 0)
                {
                    return Results.NotFound($"No se encontró un empleado con ID {id}.");
                }
            }

            // Verificar si la compañía existe
            string checkCompania = "SELECT COUNT(*) FROM Companias WHERE Id = @companiaId";
            using (var checkCompCmd = new SqlCommand(checkCompania, conn))
            {
                checkCompCmd.Parameters.AddWithValue("@companiaId", companiaId);
                int existeCompania = (int)checkCompCmd.ExecuteScalar();
                if (existeCompania == 0)
                {
                    return Results.BadRequest($"La compañía con ID {companiaId} no existe.");
                }
            }

            // Actualizar empleado
            string updateQuery = "UPDATE Empleados SET Nombre = @nombre, Edad = @edad, CompaniaId = @companiaId WHERE Id = @id";
            using (var updateCmd = new SqlCommand(updateQuery, conn))
            {
                updateCmd.Parameters.AddWithValue("@id", id);
                updateCmd.Parameters.AddWithValue("@nombre", nombre);
                updateCmd.Parameters.AddWithValue("@edad", edad);
                updateCmd.Parameters.AddWithValue("@companiaId", companiaId);

                updateCmd.ExecuteNonQuery();
                return Results.Ok($"Empleado con ID {id} actualizado correctamente.");
            }
        }
        catch (Exception ex)
        {
            return Results.Problem("Error: " + ex.Message);
        }
    }
});

app.MapDelete("/empleados/{id}", (int id) =>
{
    string connString = "Server=localhost;Database=TareaAPI;User ID=SA;Password=Sa123456;TrustServerCertificate=True;";

    using (var conn = new SqlConnection(connString))
    {
        conn.Open();

        try
        {
            string deleteQuery = "DELETE FROM Empleados WHERE Id = @id";

            using (var cmd = new SqlCommand(deleteQuery, conn))
            {
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = cmd.ExecuteNonQuery();

                if (filasAfectadas == 0)
                {
                    return Results.NotFound($"Empleado con ID {id} no encontrado.");
                }

                return Results.Ok($"Empleado con ID {id} eliminado exitosamente.");
            }
        }
        catch (Exception ex)
        {
            return Results.Problem("Error: " + ex.Message);
        }
    }
});


app.Run();
