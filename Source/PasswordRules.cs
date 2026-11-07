namespace WatchTower
{
    /// <summary>
    /// Reglas de la contraseña de DataServer, compartidas por las pestañas que la usan: solo caracteres
    /// ASCII visibles (sin espacios, tildes ni eñes), para que viaje sin ambigüedades en el HMAC.
    /// </summary>
    public static class PasswordRules
    {
        /// <summary>True si la contraseña no está vacía y todos sus caracteres son ASCII visibles.</summary>
        public static bool IsValid(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;

            foreach (char c in password)
                if (c <= ' ' || c > '~') return false;

            return true;
        }
    }
}