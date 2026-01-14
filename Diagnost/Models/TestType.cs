namespace Diagnost.Models
{
    /// <summary>
    /// Типы диагностических тестов.
    /// </summary>
    public enum TestType
    {
        /// <summary>
        /// Пробный (тренировочный) тест (col = 5)
        /// </summary>
        Trial,
        
        /// <summary>
        /// Основной (рабочий) тест (col = 30)
        /// </summary>
        Main
    }
}