namespace SwapHunter;

public static class TibetHelper
{
    public static double GetInputPrice(double input_amount, double input_reserve, double output_reserve)
    {
        if (input_amount == 0) return 0;

        var input_amount_with_fee = input_amount * 993;
        var numerator = input_amount_with_fee * output_reserve;
        var denominator = (input_reserve * 1000) + input_amount_with_fee;
        return Math.Floor((numerator / denominator));
    }

    public static double getOutputPrice(double output_amount, double input_reserve, double output_reserve)
    {
        if (output_amount > output_reserve)
        {
            return 0;
        }

        if (output_amount == 0) return 0;

        var numerator = input_reserve * output_amount * 1000;
        var denominator = (output_reserve - output_amount) * 993;
        return Math.Floor(numerator / denominator) + 1;
    }
}