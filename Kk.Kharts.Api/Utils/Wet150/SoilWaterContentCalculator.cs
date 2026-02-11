
namespace Kk.Kharts.Api.Utility.Wet150
{
    public class SoilWaterContentCalculator
    {
        private static float epsilonEau = 80.3f; // Permittivité de l'eau à 20°C
        private static Dictionary<string, (float a0, float a1, float epsilon0)> soilParameters = [];

        public static List<CalculationResult> CalcularECpVWC(float epsilonPermittivite, float ECb, float TemperatureDuSol)
        {
            InitializeSoilParameters();

            var results = Calculate(epsilonPermittivite, ECb, TemperatureDuSol);

            // Retorna a lista de resultados
            return results;
        }


        public static void InitializeSoilParameters()
        {
            soilParameters = new Dictionary<string, (float a0, float a1, float epsilon0)>
            {
                { "Minéral",           (1.6f,  8.4f,  -4.0f )},
                { "Organique",         (1.3f,  7.7f,  5.5f  )},
                { "Mélange de tourbe", (1.16f, 7.09f, 1.8f  )},
                { "Fibre de coco",     (1.16f, 7.41f, 0.0f  )},
                { "Laine minérale",    (1.04f, 7.58f, -0.3f )},
                { "Perlite",           (1.06f, 6.53f, 4.1f  )}
            };
        }


        public static List<CalculationResult> Calculate(float epsilonPermittivite, float ecBulk, float soilTemperature)
        {
            var results = new List<CalculationResult>();

            try
            {
                if (soilParameters == null)
                {
                    InitializeSoilParameters();
                }

                foreach (var soil in soilParameters!)
                {
                    string soilType = soil.Key;
                    float a0 = soil.Value.a0;
                    float a1 = soil.Value.a1;
                    float epsilon0 = soil.Value.epsilon0;                   

                    float sqrtEpsilon = (float)Math.Sqrt(epsilonPermittivite);
                    float theta = (sqrtEpsilon - a0) / a1;

                    float epsilonP = epsilonEau + (-0.37f * (soilTemperature - 20.0f));

                    float vwc = (float)Math.Round(theta * 100, 2);

                    // Normalizar VWC pour éviter une saturation irréaliste
                    if (vwc < 0.0f) vwc = 0.0f;
                    if (vwc > 100.0f) vwc = 100.0f;

                    float ecp = 0.0f;
                    if (epsilonPermittivite > epsilon0)
                    {
                        float ecpCompensatedToTemp = (epsilonP * ecBulk) / (epsilonPermittivite - epsilon0) * (1.0f - (0.02f * (soilTemperature - 25.0f)));
                        ecp = (float)Math.Round(ecpCompensatedToTemp * 0.01f, 2);
                        if (ecp < 0.0f) ecp = 0.0f;
                    }

                    results.Add(new CalculationResult
                    {
                        Permittivite = (float)Math.Round(epsilonPermittivite, 3),
                        ECb = (float)Math.Round(ecBulk, 3),
                        SoilTemperature = (float)Math.Round(soilTemperature, 2),
                        SoilType = soilType,
                        VWC = vwc,
                        ECp = ecp
                    });
                }
            }
            catch
            {
                // Retourne les résultats partiels calculés avant l'erreur
            }

            return results;
        }
    }
}