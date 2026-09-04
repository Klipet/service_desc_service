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
        uow.BeginTransaction();
        try
        {
            var workSpace = uow.GetObjectByKey<WorkSpace>(workSpaceId);
            if (workSpace == null)
            {
                uow.RollbackTransaction();
                return null;
            }

            var state = uow.Query<RoundRobinState>()
                .FirstOrDefault(s => s.WorkSpace.Oid == workSpaceId);

            if (state == null)
            {
                state = new RoundRobinState(uow) { WorkSpace = workSpace };
                state.Save();
                uow.CommitChanges();
            }

            uow.ExecuteQuery(
                "SELECT \"OID\" FROM \"RoundRobinState\" WHERE \"OID\" = @p1 FOR UPDATE",
                new object[] { state.Oid });



            state.Reload();

            var users = uow.Query<User>()
                .Where(u => u.WorkSpace.Oid == workSpaceId && u.IsActive)
                .OrderBy(u => u.Oid)
                .ToList();

            if (!users.Any())
            {
                uow.RollbackTransaction();
                return null;
            }

            // Считаем тикеты, созданные сегодня, по каждому сотруднику воркспейса
            var todayStart = DateTime.Today;
            var todayEnd = todayStart.AddDays(1);

            var todayLoad = uow.Query<Tiket>()
                .Where(t => t.User != null
                         && t.User.WorkSpace.Oid == workSpaceId
                         && t.DataCreted >= todayStart
                         && t.DataCreted < todayEnd)
                .GroupBy(t => t.User.Oid)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.UserId, x => x.Count);

            int Load(User u) => todayLoad.TryGetValue(u.Oid, out var c) ? c : 0;

            var minLoad = users.Min(Load);
            // Кандидаты — только те, у кого сегодня минимум заявок.
            // Round-robin работает уже внутри этой группы, а не по всем users,
            // иначе сотрудник с 0 заявками мог бы пропускаться курсором.
            var candidates = users.Where(u => Load(u) == minLoad).ToList();

            User selected;
            var lastUserOid = state.LastAssignedUser?.Oid;

            if (lastUserOid == null)
            {
                selected = candidates.First();
            }
            else
            {
                var lastIndexInCandidates = candidates.FindIndex(u => u.Oid == lastUserOid);

                if (lastIndexInCandidates == -1)
                {
                    // Последний назначенный не входит в группу с минимальной нагрузкой
                    // (у него уже больше заявок, чем у остальных) — просто берём первого кандидата
                    selected = candidates.First();
                }
                else
                {
                    var nextIndex = (lastIndexInCandidates + 1) % candidates.Count;
                    selected = candidates[nextIndex];
                }
            }

            state.LastAssignedUser = selected;
            state.Save();
            uow.CommitTransaction();

            return selected;
        }
        catch
        {
            uow.RollbackTransaction();
            throw;
        }
    }
}