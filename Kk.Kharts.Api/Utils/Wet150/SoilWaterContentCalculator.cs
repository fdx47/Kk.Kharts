
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

            ImprimirResultados(results);

            // Retorna a lista de resultados
            return results;
        }


        // Método para imprimir resultados
        public static void ImprimirResultados(List<CalculationResult> results)
        {
            Console.WriteLine($"\n{"Type de sol",-30} {"VWC (%)",-10} {"ECp (mS/m)",-10}");
            Console.WriteLine(new string('-', 52)); // Linha de separação

            foreach (var result in results)
            {
                Console.WriteLine($"{result.SoilType,-30} {result.VWC,-10:F1} {result.ECp,-10:F2}");
            }

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

                    if (epsilonPermittivite > epsilon0)
                    {
                        float ecpCompensatedToTemp = (epsilonP * ecBulk) / (epsilonPermittivite - epsilon0) * (1.0f - (0.02f * (soilTemperature - 25.0f)));


                        // Calcular o valor de VWC
                        float vwc = (float)Math.Round(theta * 100, 2);

                        float ecp = (float)Math.Round(ecpCompensatedToTemp * 0.01f, 2);

                        // Normalizar VWC/ECp para evitar saturação irreal (ex.: 100%)
                        if (vwc < 0.0f) vwc = 0.0f;
                        //if (vwc > 80.0f) vwc = 80.0f; // limite conservador
                         if (vwc > 100.0f) vwc = 100.0f; // limite conservador

                        if (ecp < 0.0f) ecp = 0.0f;

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
                    else
                    {

                        // Calcular o valor de VWC
                        float vwc = (float)Math.Round(theta * 100, 2);

                        // Normalizar VWC para evitar saturação irreal (ex.: 100%)
                        if (vwc < 0.0f) vwc = 0.0f;
                        // if (vwc > 80.0f) vwc = 80.0f; // limite conservador
                        if (vwc > 100.0f) vwc = 100.0f;  // limite conservador

                        results.Add(new CalculationResult
                        {
                            Permittivite = (float)Math.Round(epsilonPermittivite, 3),
                            ECb = (float)Math.Round(ecBulk, 3),
                            SoilTemperature = (float)Math.Round(soilTemperature, 2),
                            SoilType = soilType,
                            VWC = vwc,
                            ECp = 0.0f
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Une erreur s'est produite lors du calcul.{ex}", ConsoleColor.DarkRed);
            }

            return results;
        }
     

        public static float[] CalcularECpVWCOLD(float epsilonPermittivite, float ECb, float TemperatureDuSol)
        {
            epsilonPermittivite = 20;
            ECb = 10;
            TemperatureDuSol = 5;

            // Définition des paramètres typiques
            float epsilonEau = 80.3f; // Permittivité de l'eau à 20°C
            // Définition des paramètres pour chaque type de sol
            float[] a0 = { 1.6f, 1.3f, 1.16f, 1.16f, 1.04f, 1.06f }; // a0 pour Minéral, Organique, Mélange de tourbe, Fibre de coco, Laine minérale, Perlite
            float[] a1 = { 8.4f, 7.7f, 7.09f, 7.41f, 7.58f, 6.53f }; // a1 pour Minéral, Organique, Mélange de tourbe, Fibre de coco, Laine minérale, Perlite
            float[] epsilon0 = { -4.0f, 5.5f, 1.8f, 0.0f, -0.3f, 4.1f };  // Paramètres du sol pour Minéral, Organique, Mélange de tourbe, Fibre de coco, Laine minérale, Perlite

            float[] resultados = new float[3 + (a0.Length * 2)];

            // Vérifier si la permittivité est supérieure à la valeur du paramètre du sol (epsilon0)
            if (epsilonPermittivite > epsilon0.Min() && epsilonPermittivite != 0)
            {
                // Calcul de la conductivité de l'eau interstitielle (ECp non compensé)
                float epsilonP = epsilonEau + (-0.37f * (TemperatureDuSol - 20.0f)); // * Kaatze and Uhlendorf, 1981
                float ECpNonCompense = (epsilonP * ECb) / (epsilonPermittivite - epsilon0.Min()); // mS/m

                // Calcul de l'ECp compensé pour la température
                float ECpCompense = ECpNonCompense * (1.0f - (0.02f * (TemperatureDuSol - 25.0f))); // mS/m

                // Conversion de l'ECp de mS/m en mS/cm
                float ECpCompense_mS_cm = ECpCompense * 0.001f; // mS/cm

                // Initialisation d'un tableau pour stocker les résultats de chaque type de sol
                float[] thetaResults = new float[a0.Length];

                // Calculer le contenu volumétrique en eau (θ) pour chaque type de sol
                for (int i = 0; i < a0.Length; i++)
                {
                    float sqrtEpsilon = (float)Math.Sqrt(epsilonPermittivite);
                    thetaResults[i] = (sqrtEpsilon - a0[i]) / a1[i]; // Calcul du contenu volumétrique en eau pour chaque type de sol
                }

                // Affichage des résultats
                for (int i = 0; i < thetaResults.Length; i++)
                {
                    Console.WriteLine($"\nContenu volumétrique en eau en utilisant calib. Type {i + 1}: {thetaResults[i] * 100:F2}%", ConsoleColor.DarkGreen);
                }

                Console.WriteLine($"\nConductivité de l'eau interstitielle (ECp): {ECpCompense_mS_cm:F2} mS/cm \n", ConsoleColor.DarkGreen);

                // Remplir le tableau des résultats
                resultados[0] = (float)Math.Round(epsilonPermittivite, 3);
                resultados[1] = (float)Math.Round(ECb, 3);
                resultados[2] = (float)Math.Round(TemperatureDuSol, 3);

                for (int i = 0; i < thetaResults.Length; i++)
                {
                    resultados[i + 3] = (float)Math.Round(thetaResults[i] * 100, 2);
                }

                //resultados[3] = Minéral
                //resultados[4] = Organique
                //resultados[5] = Mélange de tourbe
                //resultados[6] = Fibre de coco
                //resultados[7] = Laine minérale
                //resultados[8] = Perlite

                resultados[9] = (float)Math.Round(ECpCompense_mS_cm, 2);
            }
            else
            {
                Console.WriteLine("\nErreur : La permittivité doit être supérieure au paramètre du sol (ε0) pour un calcul valide.\n", ConsoleColor.DarkRed);
                return new float[10]; // Retorna um array vazio em caso de erro
            }

            return resultados;
        }
    }
}