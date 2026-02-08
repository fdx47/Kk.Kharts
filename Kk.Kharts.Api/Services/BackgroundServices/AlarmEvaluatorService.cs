using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Shared.DTOs.Interfaces;
using Kk.Kharts.Shared.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text;
using Telegram.Bot.Types.Enums;

public class AlarmEvaluatorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    public AlarmEvaluatorService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Criar um novo scope para obter serviços com ciclo de vida scoped
                // Isto é crucial para BackgroundServices que rodam por muito tempo
                using var scope = _scopeFactory.CreateScope();
                var alarmService = scope.ServiceProvider.GetRequiredService<IAlarmRuleService>();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var telegram = scope.ServiceProvider.GetRequiredService<ITelegramService>();
                var pushoverService = scope.ServiceProvider.GetRequiredService<IPushoverService>();

                // Obter as regras de alarme ativas
                var rules = await alarmService.GetAllActiveRulesAsync();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n[Alarme Debug] Método GetActiveRulesAsync retornou {rules?.Count ?? 0} regras.");

                if (rules == null || !rules.Any())
                {
                    Console.WriteLine("[Alarme Debug] Nenhuma regra ativa encontrada para o filtro 'fdx'. Verifique a query e os dados.");
                }
                else
                {
                    foreach (var r in rules)
                    {
                        Console.WriteLine($"[Alarme Debug] Regra carregada: ID={r.Id}, DeviceId={r.DeviceId}, DevEui={r.DevEui},  Ativa={r.Enabled}, AlarmeAtivo={r.IsAlarmActive}");
                    }
                }

                var telegramAlarmMessages = new Dictionary<string, StringBuilder>();
                var telegramResetMessages = new Dictionary<string, StringBuilder>();
                // var pushoverAlarmMessages = new Dictionary<string, StringBuilder>();
                // var pushoverResetMessages = new Dictionary<string, StringBuilder>();

                // Loop através de cada regra de alarme para avaliação
                foreach (var rule in rules!)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[Alarme] Avaliando regra ID: {rule.Id}, Prop: {rule.PropertyName}");

                    IReading? latestReading = null;

                    // Busca a última leitura do sensor com base no DeviceModel
                    if (rule.DeviceModel == 7) // EM300TH
                    {
                         latestReading = await db.Em300ths
                            .Where(e => e.DeviceId == rule.DeviceId)
                            .OrderByDescending(e => e.Timestamp)
                            .FirstOrDefaultAsync(stoppingToken);               
                    }
                    else if (rule.DeviceModel == 2) // EM300DI
                    {
                        latestReading = await db.Em300Dis
                           .Where(e => e.DeviceId == rule.DeviceId)
                           .OrderByDescending(e => e.Timestamp)
                           .FirstOrDefaultAsync(stoppingToken);
                    }
                    else if (rule.DeviceModel == 47) // UC502 - Wet150
                    {                      
                        latestReading = await db.Uc502Wet150s
                            .Where(e => e.DeviceId == rule.DeviceId)
                            .OrderByDescending(e => e.Timestamp)
                            .FirstOrDefaultAsync(stoppingToken);
                    }


                    if (latestReading is null)
                    {
                        Console.WriteLine($"[Alarme Debug] Não foi encontrada leitura recente para DeviceId: {rule.DeviceId}. Pulando regra.");
                        continue; // Pula para a próxima regra se não houver leitura
                    }

                    // Usa Reflection para obter o valor da propriedade dinâmica (ex: VWC_Minerale)
                    var prop = latestReading.GetType().GetProperty(rule.PropertyName!, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (prop == null)
                    {
                        Console.WriteLine($"[Alarme Debug] ERRO: Propriedade '{rule.PropertyName}' não encontrada na entidade de leitura {latestReading.GetType().Name} para DeviceId: {rule.DeviceId}.");
                        continue; // Pula para a próxima regra se a propriedade não for encontrada
                    }

                    var valObj = prop.GetValue(latestReading);

                    if (valObj is not float currentValue) // Tenta converter o valor para float
                    {
                        Console.WriteLine($"[Alarme] Sem leitura recente para a regra ID: {rule.Id}. Pulando.");
                        continue; // Pula se a conversão falhar
                    }

                    // --- INÍCIO DA LÓGICA DE HISTÉRESE ---
                    float? hysteresis = rule.Hysteresis; // Obtém o valor de histérese da regra

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n[Alarme Debug] Valor atual para {rule.PropertyName}: {currentValue}");
                    Console.WriteLine($"[Alarme Debug] Alarme estava ativo? {rule.IsAlarmActive}, Tipo Limite Ativo: {rule.ActiveThresholdType}");
                    Console.WriteLine($"[Alarme Debug] LowValue: {rule.LowValue}, HighValue: {rule.HighValue}, Hysteresis: {rule.Hysteresis}");

                    bool flagShouldSendAlarmNotification = false; // Flag para determinar se uma NOVA notificação deve ser enviada
                    bool flagACK = false;
                    string? newActiveThresholdType = null;        // Tipo de limite que está ativo (se houver)

                    // Cenário 1: Alarme NÃO está ativo atualmente no banco (primeiro disparo)
                    if (!rule.IsAlarmActive)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\n[Alarme Debug] Entré dans le bloc : l'alarme n'est PAS active dans la base de données.");

                        // Verifica se o valor ultrapassou o limite inferior
                        if (rule.LowValue.HasValue && currentValue < rule.LowValue.Value)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"[Alarme Debug] Condição 'currentValue < LowValue' TRUE ({currentValue} < {rule.LowValue.Value}). Prêt à déclencher l'alarme LOW.");
                            flagShouldSendAlarmNotification = true;
                            newActiveThresholdType = "Low";
                        }
                        // Verifica se o valor ultrapassou o limite superior
                        else if (rule.HighValue.HasValue && currentValue > rule.HighValue.Value)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n[Alarme Debug] Condition 'currentValue > HighValue' TRUE ({currentValue} > {rule.HighValue.Value}). Prêt à déclencher l'alarme HIGH..");
                            flagShouldSendAlarmNotification = true;
                            newActiveThresholdType = "High";
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n*** [Alarme Debug] Aucune condition de seuil (Bas/Haut) n'a été atteinte lors du premier déclenchement. ***");
                        }
                    }
                    // Cenário 2: Alarme JÁ está ativo (verificar histérese ou reset)
                    else
                    {
                        // Se a regra tem histérese definida e é maior que zero
                        if (hysteresis.HasValue && hysteresis.Value > 0)
                        {
                            if (rule.ActiveThresholdType == "Low") // Alarme foi ativado por limite baixo
                            {
                                // O alarme permanece "activo" se o valor ainda está abaixo do limite + histérese
                                if (currentValue < (rule.LowValue!.Value + hysteresis.Value))
                                {
                                    newActiveThresholdType = "Low"; // Mantém o tipo de limite
                                    // Adicional: Se o valor mudar para o outro limite (alto) enquanto o alarme estava baixo, envie uma nova notificação
                                    if (rule.HighValue.HasValue && currentValue > rule.HighValue.Value)
                                    {
                                        flagShouldSendAlarmNotification = true;
                                        newActiveThresholdType = "High";
                                    }
                                }
                                // Se o valor subiu acima do limite + histérese, o alarme deve ser desativado
                                else
                                {
                                    flagShouldSendAlarmNotification = false; // Sinaliza para desativar o alarme
                                    rule.IsAlarmActive = false;
                                    rule.IsAlarmHandled = false;
                                    flagACK = true;
                                }
                            }
                            else if (rule.ActiveThresholdType == "High") // Alarme foi ativado por limite alto
                            {
                                // O alarme permanece "ativo" se o valor ainda está acima do limite - histérese
                                if (currentValue > (rule.HighValue!.Value - hysteresis.Value))
                                {
                                    newActiveThresholdType = "High"; // Mantém o tipo de limite
                                    // Adicional: Se o valor mudar para o outro limite (baixo) enquanto o alarme estava alto, envie uma nova notificação
                                    if (rule.LowValue.HasValue && currentValue < rule.LowValue.Value)
                                    {
                                        flagShouldSendAlarmNotification = true;
                                        newActiveThresholdType = "Low";
                                    }
                                }
                                // Se o valor desceu abaixo do limite - histérese, o alarme deve ser desativado
                                else
                                {
                                    flagShouldSendAlarmNotification = false; // Sinaliza para desativar o alarme
                                    rule.IsAlarmActive = false;
                                    rule.IsAlarmHandled = false;
                                    flagACK = true;
                                }
                            }
                        }
                        else // Se o alarme está ativo mas não tem histérese definida, reavalie os limites originais
                        {
                            if ((rule.LowValue.HasValue && currentValue < rule.LowValue.Value) || (rule.HighValue.HasValue && currentValue > rule.HighValue.Value))
                            {
                                // O alarme ainda está fora dos limites originais
                                // A notificação não é enviada novamente a menos que o tipo de limite tenha mudado
                                if (rule.LowValue.HasValue && currentValue < rule.LowValue.Value)
                                    newActiveThresholdType = "Low";
                                else if (rule.HighValue.HasValue && currentValue > rule.HighValue.Value)
                                    newActiveThresholdType = "High";
                            }
                            else
                            {
                                // O alarme estava ativo mas o valor voltou para a faixa normal, desativa
                                flagShouldSendAlarmNotification = false; // Sinaliza para desativar o alarme
                            }
                        }
                    }
                    // --- FIM DA LÓGICA DE HISTÉRESE ---


                    // --- INÍCIO: AÇÕES DE NOTIFICAÇÃO E ATUALIZAÇÃO DE ESTADO ---

                    // Condição para ENVIAR uma NOVA notificação de ALARME
                    // Envia se:
                    // 1. shouldSendAlarmNotification é true (primeiro disparo ou mudança de limite)
                    // OU
                    // 2. O alarme NÃO estava ativo E o valor violou um dos limites originais (caso a histérese seja 0 ou nula e o valor ainda esteja fora)

                    Console.WriteLine($"\n[Alarme Debug] Fin de la logique d’hystérésis. shouldSendAlarmNotification = {flagShouldSendAlarmNotification}, newActiveThresholdType = {newActiveThresholdType ?? "NULO"}");
                   
                    if (flagShouldSendAlarmNotification || (!rule.IsAlarmActive && ((rule.LowValue.HasValue && currentValue < rule.LowValue.Value) || (rule.HighValue.HasValue && currentValue > rule.HighValue.Value))))
                    {
                        Console.WriteLine($"[Alarme Debug] La condition finale de DÉCLENCHEMENT de l'alarme est TRUE pour la règle ID : {rule.Id}.");

                        var thresholdValue = newActiveThresholdType == "Low" ? rule.LowValue : rule.HighValue;

                 
                        // Converte DeviceId para string antes de usá-lo como chave do dicionário.
                        string deviceIdString = rule.DeviceId.ToString();
                        // Garante que rule.Device.Name é tratado como string e lida com nulos (para o título da mensagem).
                        string deviceDescriptionForHtml = rule.Description?.ToString() ?? "* Device inconnu *";
                        // Garante que rule.PropertyName é tratado como string e lida com nulos antes de chamar ToFriendlyName.
                        string propertyNameForHtml = rule.PropertyName?.ToString() ?? "* Variable inconnue *";
                        string friendlyPropertyName = ToFriendlyName(propertyNameForHtml); // Chama ToFriendlyName com uma string segura

                        // Adiciona a mensagem de ALARME à lista de mensagens para o dispositivo
                        if (!telegramAlarmMessages.ContainsKey(deviceIdString))
                        {
                            telegramAlarmMessages[deviceIdString] = new StringBuilder($"<b><u>⚠️ ALERTE KropKontrol pour la sonde:</u></b><b>\n\n{telegram.EscapeHtml(deviceDescriptionForHtml)}</b> \n");
                        }
                       
                        var unit = GetUnit(propertyNameForHtml);
                        telegramAlarmMessages[deviceIdString].Append($"\n<b>• {telegram.EscapeHtml(friendlyPropertyName)}</b> - Dernière lecture = {currentValue:F0}{unit} {(currentValue < thresholdValue ? "&lt;" : "&gt;")} seuil {thresholdValue:F0}{unit}\n");
                     
                        rule.IsAlarmActive = true;  // Atualiza o estado da regra para ativo
                        rule.ActiveThresholdType = newActiveThresholdType ?? string.Empty;
                    }
                    // Condição para ENVIAR uma notificação de RESET de alarme
                    // Envia se: O alarme estava ativo mas agora está "limpo" (fora da zona de histérese ou voltou ao normal)

                    //else if (rule.IsAlarmActive && !flagShouldSendAlarmNotification && !rule.IsAlarmHandled)
                    else if (flagACK)
                            {
                        Console.WriteLine($"\n[Alarme Debug] Condição de RESET de alarme é TRUE para regra ID: {rule.Id}. Enviando notificação de reset.");

                        // Converte DeviceId para string antes de usá-lo como chave do dicionário.
                        string deviceIdString = rule.DeviceId.ToString();
                        // Garante que rule.Device.Name é tratado como string e lida com nulos (para o título da mensagem).
                        string deviceDescriptionForHtml = rule.Description?.ToString() ?? "- Device inconnu -";
                        // Garante que rule.PropertyName é tratado como string e lida com nulos antes de chamar ToFriendlyName.
                        string propertyNameForHtml = rule.PropertyName?.ToString() ?? "- Variable inconnue -";
                        string friendlyPropertyName = ToFriendlyName(propertyNameForHtml); // Chama ToFriendlyName com uma string segura

                        if (!telegramResetMessages.ContainsKey(deviceIdString))
                        {
                            telegramResetMessages[deviceIdString] = new StringBuilder($"<b><u>✅ KropKontrol - La valeur est revenue dans les paramètres normaux.</u></b>\n\n{telegram.EscapeHtml(deviceDescriptionForHtml)}");
                        }

                        string unidade = GetUnit(rule.PropertyName ?? "");
                        telegramResetMessages[deviceIdString].Append($"\n\n- Variable: <b>{telegram.EscapeHtml(friendlyPropertyName)}</b> - Valeur actuelle : {currentValue:F0}{unidade}\n");

                        //rule.IsAlarmActive = false;      // Desativa o estado da regra
                        //rule.ActiveThresholdType = null; // Reseta o tipo de limite
                        //rule.IsAlarmHandled = true;      // Desativa o estado da regra
                        flagACK = false;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine($"\n[Alarme Debug] Nenhuma condição de disparo OU reset foi atendida para regra ID: {rule.Id}.");
                    }



                    var entityRule = new AlarmRule
                    {
                        Id = rule.Id,
                        DevEui = rule.DevEui,
                        DeviceId = rule.DeviceId,
                        Description = rule.Description ?? "",
                        PropertyName = rule.PropertyName ?? "",
                        LowValue = rule.LowValue,
                        HighValue = rule.HighValue,
                        Hysteresis = rule.Hysteresis,
                        Enabled = rule.Enabled,
                        DeviceModel = rule.DeviceModel,
                        IsAlarmActive = rule.IsAlarmActive,
                        ActiveThresholdType = rule.ActiveThresholdType,
                        IsAlarmHandled = rule.IsAlarmHandled,
                        UserAlarmRules = new List<UserAlarmRule>()
                    };

                    db.AlarmRules.Update(entityRule);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\n[Alarme Debug] Regra ID {rule.Id} marcada para atualização no DB. IsAlarmActive={rule.IsAlarmActive}, ActiveThresholdType={rule.ActiveThresholdType ?? "NULO"}");
                }

                // --- Envia notificações agrupadas APÓS o loop de avaliação das regras ---
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n[Alarme Debug] Enviando notificações Telegram agrupadas...");

                // Itera sobre as mensagens de ALARME agrupadas por dispositivo e as envia.
                foreach (var entry in telegramAlarmMessages)
                {
                    var deviceIdString = entry.Key;
                    var messageContent = entry.Value.ToString();
                    try
                    {
                        await telegram.SendMessageAsync(messageContent, ParseMode.Html);
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine($"\n[Telegram Debug] Notificação de ALARME para DeviceId {deviceIdString} ENVIADA com sucesso.");
                    }
                    catch (Exception telegramEx)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\n[Telegram Debug] ERRO ao enviar notificação de ALARME para DeviceId {deviceIdString}: {telegramEx.Message}");
                    }
                }

                // Itera sobre as mensagens de RESET agrupadas por dispositivo e as envia.
                foreach (var entry in telegramResetMessages)
                {
                    var deviceIdString = entry.Key;
                    var messageContent = entry.Value.ToString();
                    try
                    {
                        await telegram.SendMessageAsync(messageContent, ParseMode.Html);
                        //Console.ForegroundColor = ConsoleColor.DarkYellow;
                        //Console.WriteLine("***********************************************************************************");
                        //Console.WriteLine($"*** [Telegram Debug] Notificação de RESET para DeviceId {deviceIdString} ENVIADA com sucesso. ***");
                        //Console.ForegroundColor = ConsoleColor.White;
                        //Console.WriteLine($"Mesage : {messageContent}");
                        //Console.ForegroundColor = ConsoleColor.DarkYellow;
                        //Console.WriteLine("***********************************************************************************");
                        //Console.ResetColor();
                        //Console.ForegroundColor = ConsoleColor.Yellow;
                    }
                    catch (Exception telegramEx)
                    {
                        Console.WriteLine($"[Telegram Debug] ERRO ao enviar notificação de RESET para DeviceId {deviceIdString}: {telegramEx.Message}");
                    }
                }               

                // Salva todas as alterações de estado das regras no banco de dados em uma única transação
                Console.WriteLine("[Alarme Debug] Loop de avaliação de regras finalizado. Salvando alterações no DB.");
                await db.SaveChangesAsync(stoppingToken);
                Console.WriteLine("[Alarme Debug] Alterações salvas no DB.\n\n");
               
               await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
               // await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);  // Atraso antes da próxima execução

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Alarme] Erro no serviço de avaliação: {ex.Message}");              
                var delay = GetDelayForException(ex);
                await Task.Delay(delay, stoppingToken); // Aguarda antes de tentar novamente
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }


    private static TimeSpan GetDelayForException(Exception exception)
    {
        if (exception is SqlException)
        {
            return TimeSpan.FromMinutes(2);
        }

        if (exception is DbUpdateException { InnerException: SqlException })
        {
            return TimeSpan.FromMinutes(2);
        }

        if (exception is TimeoutException)
        {
            return TimeSpan.FromSeconds(30);
        }

        return TimeSpan.FromSeconds(5);
    }


    private string ToFriendlyName(string propertyName)
    {
        return propertyName
            .Replace("permittivite", "Permittivité")
            .Replace("soilTemperature", "Température du Sol (°C)")
            .Replace("mineralVWC", "VWC - Minérale")
            .Replace("organicVWC", "VWC - Organique")
            .Replace("peatMixVWC", "VWC - Mélange de Tourbe")
            .Replace("coirVWC", "VWC - Fibre de Coco")
            .Replace("minWoolVWC", "VWC - Laine Minérale")
            .Replace("perliteVWC", "VWC - Perlite")

            .Replace("mineralECp", "Ec Minérale (ECp)")
            .Replace("organicECp", "Ec Organique (ECp)")
            .Replace("organicECp", "Ec Mélange de Tourbe (ECp)")
            .Replace("coirECp", "Ec Fibre de Coco (ECp)")
            .Replace("minWoolECp", "Ec Laine Minérale (ECp)")
            .Replace("perliteECp", "Ec Perlite (ECp)")
            .Replace("temperature", "Température (°C)")
            .Replace("humidity", "Humidité (%)")
            .Replace("battery", "Batterie %")
            .Replace("battery", "Batterie %")
            .Replace("_", " ");
    }


    private string GetUnit(string propertyName)
    {
        propertyName = propertyName.ToLower();

        return propertyName switch
        {
            "soiltemperature" => "°C",
            "temperature" => "°C",
            "humidity" => "%",
            "battery" => "%",
            "mineralvwc" => "%",
            "organicvwc" => "%",
            "peatmixvwc" => "%",
            "coirvwc" => "%",
            "minwoolvwc" => "%",
            "perlitevwc" => "%",
            "mineralecp" => "mS/cm",
            "organicecp" => "mS/cm",
            "coirecp" => "mS/cm",
            "minwoolecp" => "mS/cm",
            "perliteecp" => "mS/cm",
            _ => ""  //vazio se não achar unidade
        };
    }


    public string EscapeHtml(string text)
    {
        // Ordem importa
        text = text.Replace("&", "&amp;"); // Este deve ser o primeiro
        text = text.Replace("<", "&lt;");
        text = text.Replace(">", "&gt;");
        return text;
    }
}


