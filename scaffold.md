# Scaffold Command for Entity Framework Core

This command is used to generate models and DbContext for an existing database using Entity Framework Core.

### Default Scaffold Command:
```bash
Scaffold-DbContext "Server=(LocalDB)\MSSQLLocalDB;Database=AsiBasecodeDb;Integrated Security=False;Trusted_Connection=True" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -ContextDir . -F

Scaffold-DbContext "{connectionstring here}" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -ContextDir . -F
