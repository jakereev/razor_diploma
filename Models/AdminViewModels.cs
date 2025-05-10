using System.Collections.Generic;

namespace AlphaMaterials.Models
{
    /// <summary>
    /// Для списка пользователей в таблице /Admin/Users
    /// </summary>
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
    }

    /// <summary>
    /// Одна строка с чекбоксом для роли
    /// </summary>
    public class RoleCheckbox
    {
        public string RoleName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// Модель для страницы /Admin/EditRoles
    /// </summary>
    public class EditRolesViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<RoleCheckbox> Roles { get; set; } = new();
    }
}
