using Amazon.Runtime.Internal;
using DevExpress.Xpo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using static DevExpress.Data.Helpers.ExpressiveSortInfo;

[ApiController]
[Route("[controller]")]
public class UserController: ControllerBase
{
    private readonly UnitOfWork _uow;
    private readonly GenerateJwtToken _jwtGenerator;

    public UserController(UnitOfWork uow, GenerateJwtToken jwtGenerator)
    {
        _uow = uow;
        _jwtGenerator = jwtGenerator;
    }

    private User CurrentUser => (User)HttpContext.Items["CurrentUser"]!;

    // Проверка права
    private bool HasPermission(string code) =>
        CurrentUser?.RoleUser?.RolePermissions
            .Any(rp => rp.Permission.Name == code
                    && rp.Permission.IsActive) ?? false;

    [HttpGet]
    [ApiKey]
    public IActionResult GetAllUser()
    {
        if (!HasPermission(PermisionConstant.UserRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {
            var users = _uow.Query<User>().ToList();
            var resault = users.Select(u => new UserResponseDto
            {
                Oid = u.Oid,
                Name = u.Name,
                Email = u.Email,
                FirstName = u.FirstName,
                Phone = u.Phone,
                Loghin = u.Loghin,
                DateCreated = u.DateCreated,
                WorkSpaceName = u.WorkSpace.Name,
                WorkSpaceId = u.WorkSpace.Oid,
                ApiKey = u.ApiKey
                

            }).ToList();
            return Ok(resault);
        }
        catch (Exception ex) {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }

    [HttpPost]
    [ApiKey]
    public IActionResult CreateUser([FromBody] UserDto userModel) {
        if (!HasPermission(PermisionConstant.UserCreate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });

        if (userModel == null)
            return BadRequest("Model is null");

        var wp = _uow.Query<WorkSpace>().FirstOrDefault(u => u.Oid == userModel.WorkSpaceId);
        if (wp == null)
            return NotFound("Рабочее место не найдено");

        var user = new User(_uow)
        {
            Name = userModel.Name,
            Email = userModel.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userModel.Password),
            FirstName = userModel.FirstName,
            Phone = userModel.Phone,
            Loghin = userModel.Loghin,
            DateCreated = DateTime.UtcNow,
            WorkSpace = wp

        };
        try
        {
            _uow.CommitChanges();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error saving to database: {ex.Message}");
        }
        var resaultUser = new UserResponseDto
        {
            Oid = user.Oid,
            Name = user.Name,
            Email = user.Email,
            FirstName = user.FirstName,
            Phone = user.Phone,
            WorkSpaceName = user.WorkSpace.Name,
            DateCreated = DateTime.UtcNow,
           
        };

        return Ok(resaultUser);
    }

    [HttpPut("Update[controller]")]
    [ApiKey]
    public IActionResult UpdateUser([FromQuery]int id, [FromBody] UserDto userModel) 
    {
        if (!HasPermission(PermisionConstant.UserUpdate))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        try
        {
            var user = _uow.Query<User>().FirstOrDefault(u => u.Oid == id);
            if (user == null) return NotFound($"User Not found {id} incorect");

            var wp = _uow.GetObjectByKey<WorkSpace>(user.WorkSpace);
            if (wp == null)
                return NotFound("Пользователь не найден");

            user.Name = userModel.Name;
            user.Email = userModel.Email;
            user.FirstName = userModel.FirstName;
            user.Phone = userModel.Phone;
            user.PasswordHash = userModel.Password;
            user.Loghin = userModel.Loghin;
            user.WorkSpace = wp;
            _uow.CommitChanges();
            return Ok($"User {id}, {user.Name} is update");
        }
        catch (Exception ex) 
        {
            return StatusCode(ex.HResult, $"Ошибка при обновлении: {ex.Message}");
        }
    }


    [HttpGet("Get[controller]ById")]
    [ApiKey]
    public IActionResult UserGetById([FromQuery]int id) 
    {
        if (!HasPermission(PermisionConstant.UserRead))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });
        var user = _uow.GetObjectByKey<User>(id);
        if (user == null) return NotFound("User not found");

        var wp = _uow.GetObjectByKey<WorkSpace>(user.WorkSpace);
        if (wp == null)
            return NotFound("Пользователь не найден");
        try
        {


            var responseUser = new UserResponseDto
            {
                Oid = user.Oid,
                Name = user.Name,
                Email = user.Email,
                FirstName = user.FirstName,
                Phone = user.Phone,
                Loghin = user.Loghin,
                WorkSpaceName = wp.Name,
                DateCreated = user.DateCreated,
                ApiKey = user.ApiKey

            };
            return Ok(responseUser);
        }
        catch (Exception ex) {
            return StatusCode(ex.HResult, $"User error: {ex.Message}");
        }
    }

    [HttpPost("Auth")]
    [AllowAnonymous]
    public IActionResult AuthUser([FromBody] LoginRequestDto reqest)
    {
        if (string.IsNullOrWhiteSpace(reqest.Login) || string.IsNullOrWhiteSpace(reqest.Password))
            return BadRequest("Login and password are required");

        var user = _uow.Query<User>().FirstOrDefault(u => u.Loghin == reqest.Login);

        if (user == null || !BCrypt.Net.BCrypt.Verify(reqest.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        try
        {
            user.ApiKey = ApiKeyGenerator.Generate();
            _uow.CommitChanges();
            return Ok(new
            {
                apikey = user.ApiKey,
                user = new
                {
                    id = user.Oid,
                    name = user.Name,
                    email = user.Email,
                    login = user.Loghin,
                    permissions = user.RoleUser?.RolePermissions.Select(rp => new
            {
                id = rp.Permission.Oid,
                name = rp.Permission.Name
            }) ?? Enumerable.Empty<object>()
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Internal server error",
                detail = ex.Message  // убрать в продакшене
            });
        }
    }


    [HttpDelete("Delete[controller]ById")]
    [ApiKey]
    public IActionResult DeleteUser([FromQuery] int id) {
        if (!HasPermission(PermisionConstant.UserDelete))
            return StatusCode(403, new { error = "Access denied", errorCode = 403 });

        try
        {
            var user = _uow.Query<User>().FirstOrDefault(u => u.Oid == id);
            if (user == null) return NotFound($"User {id} is incorrect");
            _uow.Delete(user);
            _uow.CommitChanges();
            return Ok("User deleted");
        }
        catch(Exception ex) 
        {
            return StatusCode(ex.HResult, ex.Message);
        }
    }


    public static class ApiKeyGenerator
    {
        public static string Generate()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("=", "")
                .Replace("+", "")
                .Replace("/", "");
        }
    }


}