using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;
using static DevExpress.Data.Helpers.ExpressiveSortInfo;

[ApiController]
[Route("api/[controller]")]
public class UserController: ControllerBase
{
    private readonly UnitOfWork _uow;
    public UserController(UnitOfWork uow)
    {
        _uow = uow;
    }
    [HttpGet]
    public IActionResult GetAllUser()
    {
        try
        {
            var users = _uow.Query<User>().ToList();
            var resault = users.Select(u => new UserDto
            {
                Oid = u.Oid,
                Name = u.Name,
                Email = u.Email,
                FirstName = u.FirstName,
                Password = u.Password,
                Phone = u.Phone,
                Loghin = u.Loghin,
                DateCreated = u.DateCreated,
                WorkSpaceName = u.WorkSpace.Name,
                WorkSpaceId = u.WorkSpace.Oid
            }).ToList();
            return Ok(resault);
        }
        catch (Exception ex) {
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }

    [HttpPost]
    public IActionResult CreateUser([FromBody] UserDto userModel) {
        if (userModel == null)
            return BadRequest("Model is null");

        var wp = _uow.Query<WorkSpace>().FirstOrDefault(u => u.Oid == userModel.WorkSpaceId);
        if (wp == null)
            return NotFound("Пользователь не найден");

        var user = new User(_uow)
        {
            Oid = userModel.Oid,
            Name = userModel.Name,
            Email = userModel.Email,
            FirstName = userModel.FirstName,
            Password = userModel.Password,
            Phone = userModel.Phone,
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
        var resaultUser = new UserDto
        {
            Oid = user.Oid,
            Name = user.Name,
            Email = user.Email,
            FirstName = user.FirstName,
            Password = user.Password,
            Phone = user.Phone,
            WorkSpaceName = user.WorkSpace.Name,
            DateCreated = DateTime.UtcNow,
        };

        return Ok(resaultUser);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateUser(int id, [FromBody] UserDto userModel) 
    {
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
            user.Password = userModel.Password;
            user.Phone = userModel.Phone;
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
    [HttpGet("{id}")]
    public IActionResult UserGetById(int id) 
    {
        var user = _uow.GetObjectByKey<User>(id);
        if (user == null) return NotFound("User not found");

        var wp = _uow.GetObjectByKey<WorkSpace>(user.WorkSpace);
        if (wp == null)
            return NotFound("Пользователь не найден");
        try
        {


            var responseUser = new UserDto
            {
                Oid = user.Oid,
                Name = user.Name,
                Email = user.Email,
                FirstName = user.FirstName,
                Password = user.Password,
                Phone = user.Phone,
                Loghin = user.Loghin,
                WorkSpaceName = wp.Name,
                DateCreated = user.DateCreated

            };
            return Ok(responseUser);
        }
        catch (Exception ex) {
            return StatusCode(ex.HResult, $"User error: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteUser(int id) {
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

}