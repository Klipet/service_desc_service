using DevExpress.Xpo;

public class SettingTiket
{

    private static int _lastUserIndex = 0;
    private static readonly object _lock = new object();
    public static User GetUserByLoad(UnitOfWork uow, int workSpaceId)
    {
        var users = uow.Query<User>()
            .Where(u => u.WorkSpace.Oid == workSpaceId)
            .ToList();
        Console.WriteLine($"User: {users.Count}");
        if (!users.Any()) return null;

        // Считаем нагрузку явно через запрос к тикетам
        var userLoad = users.Select(u => new
        {
            User = u,
            Load = uow.Query<Tiket>().Count(t => t.User.Oid == u.Oid && t.State.Oid == 1)
        }).ToList();

        // Лог чтобы видеть нагрузку каждого
        foreach (var item in userLoad)
        {
            Console.WriteLine($"User: {item.User.Name}, Load: {item.Load}");
        }

        return userLoad
            .OrderBy(x => x.Load)
            .First()
            .User;
    }
    public static User GetUserRoundRobin(UnitOfWork uow, int workSpaceId, ILogger<TiketController> logger)
    {
        logger.LogError("[RoundRobin] Старт подбора пользователя для WorkSpaceId: {WorkSpaceId}", workSpaceId);
        var users = uow.Query<User>()
            .Where(u => u.WorkSpace.Oid == workSpaceId)
            .OrderBy(u => u.Oid)
            .ToList();
        logger.LogError("[RoundRobin] Найдено пользователей в воркспейсе {WorkSpaceId}: {Count}", workSpaceId, users.Count);
        if (!users.Any())
        {
            logger.LogError("[RoundRobin] Список пользователей для WorkSpaceId {WorkSpaceId} пуст! Возвращаем null.", workSpaceId);
            return null;
        }
        var lastTicket = uow.Query<Tiket>()
            .Where(t => t.WorkSpace.Oid == workSpaceId) // только в этом воркспейсе
            .OrderByDescending(t => t.Oid)
            .FirstOrDefault();
        
        // Если тикетов ещё нет — назначаем первого
        if (lastTicket == null)
        {
            var firstUser = users.First();
            logger.LogError("[RoundRobin] Предыдущие тикеты не найдены. Назначаем первого пользователя: {UserOid} ({UserName})",
                firstUser.Oid, firstUser.Name);
            return firstUser;
        }

        int? lastUserOid = lastTicket.User?.Oid;
        logger.LogError("[RoundRobin] Последний тикет ID: {TicketOid}. Закрепленный за ним User ID: {LastUserOid}",
            lastTicket.Oid, lastUserOid.HasValue ? lastUserOid.Value.ToString() : "NULL");

        if (lastUserOid == null)
        {
            var firstUser = users.First();
            logger.LogError("[RoundRobin] У последнего тикета не был указан пользователь. Назначаем первого из списка: {UserOid} ({UserName})",
                firstUser.Oid, firstUser.Name);
            return firstUser;
        }

        var lastIndex = users.FindIndex(u => u.Oid == lastTicket.User.Oid);

        // Если пользователь не найден в списке — назначаем первого
        if (lastIndex == -1)
            return users.First();

        var nextIndex = (lastIndex + 1) % users.Count;
        var user = users[nextIndex];
        logger.LogError("[RoundRobin] Успешно распределено по кругу. Индекс предыдущего: {LastIndex}, Индекс нового: {NextIndex}. Назначен: {UserOid} ({UserName})",
        lastIndex, nextIndex, user.Oid, user.Name);
        return user;
    }

}