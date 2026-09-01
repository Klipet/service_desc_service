using DevExpress.Xpo;

public class SettingTiket
{
    public static User GetUserByLoad(UnitOfWork uow, int workSpaceId, ILogger logger)
    {
        var users = uow.Query<User>()
            .Where(u => u.WorkSpace.Oid == workSpaceId)
            .ToList();
        logger.LogInformation("User: {Count}", users.Count);
        if (!users.Any()) return null;

        var userLoad = users.Select(u => new
        {
            User = u,
            Load = uow.Query<Tiket>().Count(t => t.User.Oid == u.Oid && t.State.Oid == 1)
        }).ToList();

        foreach (var item in userLoad)
        {
            logger.LogInformation("User: {UserName}, Load: {Load}", item.User.Name, item.Load);
        }

        return userLoad.OrderBy(x => x.Load).First().User;
    }

    public static User GetUserRoundRobin(UnitOfWork uow, int workSpaceId, ILogger<TiketController> logger)
    {
        logger.LogInformation("[RoundRobin] Старт подбора пользователя для WorkSpaceId: {WorkSpaceId}", workSpaceId);

        var users = uow.Query<User>()
            .Where(u => u.WorkSpace.Oid == workSpaceId)
            .OrderBy(u => u.Oid)
            .ToList();
        logger.LogInformation("[RoundRobin] Найдено пользователей в воркспейсе {WorkSpaceId}: {Count}", workSpaceId, users.Count);

        if (!users.Any())
        {
            logger.LogWarning("[RoundRobin] Список пользователей для WorkSpaceId {WorkSpaceId} пуст! Возвращаем null.", workSpaceId);
            return null;
        }

        var lastTicket = uow.Query<Tiket>()
            .Where(t => t.WorkSpace.Oid == workSpaceId && t.User != null) // 👈 заодно чинит и предыдущую проблему
            .OrderByDescending(t => t.Oid)
            .FirstOrDefault();

        if (lastTicket == null)
        {
            var firstUser = users.First();
            logger.LogInformation("[RoundRobin] Предыдущие тикеты не найдены. Назначаем первого пользователя: {UserOid} ({UserName})",
                firstUser.Oid, firstUser.Name);
            return firstUser;
        }

        int? lastUserOid = lastTicket.User?.Oid;
        logger.LogInformation("[RoundRobin] Последний тикет ID: {TicketOid}. Закреплённый за ним User ID: {LastUserOid}",
            lastTicket.Oid, lastUserOid.HasValue ? lastUserOid.Value.ToString() : "NULL");

        var lastIndex = users.FindIndex(u => u.Oid == lastUserOid);
        if (lastIndex == -1)
        {
            var firstUser = users.First();
            logger.LogInformation("[RoundRobin] Пользователь последнего тикета не найден в списке. Назначаем первого: {UserOid} ({UserName})",
                firstUser.Oid, firstUser.Name);
            return firstUser;
        }

        var nextIndex = (lastIndex + 1) % users.Count;
        var user = users[nextIndex];
        logger.LogInformation("[RoundRobin] Успешно распределено по кругу. Индекс предыдущего: {LastIndex}, Индекс нового: {NextIndex}. Назначен: {UserOid} ({UserName})",
            lastIndex, nextIndex, user.Oid, user.Name);
        return user;
    }
}