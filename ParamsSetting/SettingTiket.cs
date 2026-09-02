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

        uow.BeginTransaction();
        {
            try
            {
                var workSpace = uow.GetObjectByKey<WorkSpace>(workSpaceId);
                if (workSpace == null)
                {
                    Console.WriteLine($"[RoundRobin] WorkSpace {workSpaceId} не найден.");
                    uow.RollbackTransaction();
                    return null;
                }

                // Ищем состояние курсора для этого воркспейса
                var state = uow.Query<RoundRobinState>()
                    .FirstOrDefault(s => s.WorkSpace.Oid == workSpaceId);

                if (state == null)
                {
                    Console.WriteLine($"[RoundRobin] Состояние для WorkSpaceId {workSpaceId} не найдено, создаём новое.");
                    state = new RoundRobinState(uow) { WorkSpace = workSpace };
                    state.Save();
                }

                var users = uow.Query<User>()
                    .Where(u => u.WorkSpace.Oid == workSpaceId)
                    .OrderBy(u => u.Oid)
                    .ToList();

                Console.WriteLine($"[RoundRobin] Найдено пользователей в воркспейсе {workSpaceId}: {users.Count}");

                if (!users.Any())
                {
                    Console.WriteLine($"[RoundRobin] Список пользователей пуст! Возвращаем null.");
                    uow.RollbackTransaction();
                    return null;
                }

                User selected;
                var lastUserOid = state.LastAssignedUser?.Oid;

                if (lastUserOid == null)
                {
                    selected = users.First();
                    Console.WriteLine($"[RoundRobin] Курсор пуст. Назначаем первого: {selected.Oid} ({selected.Name})");
                }
                else
                {
                    var lastIndex = users.FindIndex(u => u.Oid == lastUserOid);
                    var nextIndex = (lastIndex == -1) ? 0 : (lastIndex + 1) % users.Count;
                    selected = users[nextIndex];
                    Console.WriteLine($"[RoundRobin] Индекс предыдущего: {lastIndex}, индекс нового: {nextIndex}. Назначен: {selected.Oid} ({selected.Name})");
                }

                // Обновляем курсор и сохраняем в той же транзакции
                state.LastAssignedUser = selected;
                state.Save();

                uow.CommitTransaction();
                Console.WriteLine($"[RoundRobin] Курсор обновлён, транзакция закоммичена.");
                return selected;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoundRobin] Ошибка: {ex.Message}. Откат транзакции.");
                uow.RollbackTransaction();
                throw;
            }
        }
    }

}