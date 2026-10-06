# HOSTAGE-DUCK: con tin tu cui sau 3 s
- EnemyConfig.hostageExposeTime default 4 -> 3 (cs + EnemyConfig_Default.asset).
- EncounterWave.hostagesStandStill default true -> false (tooltip cap nhat). Builder khong set field nen dung lai se ra false.
- Scene Level_01..04 + Level_Chain: 9/12/12/12/45 wave doi stand-still 1 -> 0 (qua execute_code/SerializedObject, da save). Sandbox gameplay-coder-enemy.unity khong dong toi.
- Khong them co "con tin co dinh" rieng: co wave-level hostagesStandStill=true van dung cho wave can giu dung im (design chua co con tin troi ghe).
- Test EditMode ClaudeCop.Enemy.Tests: 50/50 pass (khong test nao hong). Validate Level 01..04 + Chain: PASS 0 FAIL 0 WARN (tam nhin 0 tia bi chan). Validator khong coi con tin la vat chan nen khong chinh gi.
