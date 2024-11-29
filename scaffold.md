# Scaffold Command for Entity Framework Core

This command is used to generate models and DbContext for an existing database using Entity Framework Core.

### Default Scaffold Command:
```bash
Scaffold-DbContext "{Addr=(LocalDB)\MSSQLLocalDB;database=AsiBasecodeDb;Integrated Security=False;Trusted_Connection=True}" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -ContextDir . -F

### Custom Scaffold Command:
```bash
Scaffold-DbContext "{connectionstring here}" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -ContextDir . -F
