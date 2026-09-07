using UnityEngine;

// Общий сигнал «игрок взаимодействует с квестовым предметом».
//
// Нужен подсказкам: следить за позицией объекта не годится там, где
// предмет движется сам — облака в Q2 всё время дрейфуют по траекториям,
// и по их координатам не отличить дрейф от перетаскивания.
//
// Скрипты предметов дёргают Touch() каждый кадр, пока их держат.
// Сколько облаков или зверьков в сцене — неважно, сигнал один на всех.
public static class PlayerActivity
{
    // Время последнего касания по неигровым часам: работает и на паузе
    public static float LastTouchTime { get; private set; } = -999f;

    public static void Touch()
    {
        LastTouchTime = Time.unscaledTime;
    }

    public static float SecondsSinceTouch => Time.unscaledTime - LastTouchTime;

    // Новая игра — забываем прошлые касания
    public static void Reset()
    {
        LastTouchTime = -999f;
    }
}
