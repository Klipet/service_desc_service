using DevExpress.Xpo;

public class SettingTiket
{
    public static User GetUserByLoad(UnitOfWork uow, int workSpaceId)
    {
        var users = uow.Query<User>()
            .Where(u => u.WorkSpace.Oid == workSpaceId)
            .ToList();

        Console.WriteLine($"[GetUserByLoad] Найдено пользователей: {users.Count}");

        if (!users.Any()) return null;

        var userLoad = users.Select(u => new
        {
            User = u,
            Load = uow.Query<Tiket>().Count(t => t.User.Oid == u.Oid && t.State.Oid == 1)
        }).ToList();

        foreach (var item in userLoad)
        {
            Console.WriteLine($"[GetUserByLoad] User: {item.User.Name} (Oid={item.User.Oid}), Load: {item.Load}");
        }

        var result = userLoad.OrderBy(x => x.Load).First().User;
        Console.WriteLine($"[GetUserByLoad] Выбран: {result.Name} (Oid={result.Oid})");
        return result;
    }

    public static User GetUserRoundRobin(UnitOfWork uow, int workSpaceId)
    {
        Console.WriteLine($"[RoundRobin] Старт подбора пользователя для WorkSpaceId: {workSpaceId}");

        var users = uow.Query<User>()
            .Where(u => u.WorkSpace.Oid == workSpaceId)
            .OrderBy(u => u.Oid)
            .ToList();

        Console.WriteLine($"[RoundRobin] Найдено пользователей в воркспейсе {workSpaceId}: {users.Count}");

        if (!users.Any())
        {
            Console.WriteLine($"[RoundRobin] Список пользователей для WorkSpaceId {workSpaceId} пуст! Возвращаем null.");
            return null;
        }

        var lastTicket = uow.Query<Tiket>()
            .Where(t => t.WorkSpace.Oid == workSpaceId && t.User != null)
            .OrderByDescending(t => t.Oid)
            .FirstOrDefault();

        if (lastTicket == null)
        {
            var firstUser = users.First();
            Console.WriteLine($"[RoundRobin] Предыдущие тикеты не найдены. Назначаем первого пользователя: {firstUser.Oid} ({firstUser.Name})");
            return firstUser;
        }

        int? lastUserOid = lastTicket.User?.Oid;
        Console.WriteLine($"[RoundRobin] Последний тикет ID: {lastTicket.Oid}. Закреплённый за ним User ID: {(lastUserOid.HasValue ? lastUserOid.Value.ToString() : "NULL")}");

        var lastIndex = users.FindIndex(u => u.Oid == lastUserOid);
        if (lastIndex == -1)
        {
            var firstUser = users.First();
            Console.WriteLine($"[RoundRobin] Пользователь последнего тикета не найден в списке. Назначаем первого: {firstUser.Oid} ({firstUser.Name})");
            return firstUser;
        }

        var nextIndex = (lastIndex + 1) % users.Count;
        var user = users[nextIndex];

        Console.WriteLine($"[RoundRobin] Успешно распределено по кругу. Индекс предыдущего: {lastIndex}, Индекс нового: {nextIndex}. Назначен: {user.Oid} ({user.Name})");
        return user;
    }
}