/* Generated file. Don't edit. */

using System.Collections.Generic;

namespace ModbusImportSample
{
	public enum Tag
	{
		hmi_cmd, hmi_arg0, hmi_arg1, hmi_tension, hmi_pos, hmi_pos_setpoint, hmi_speed_lim, hmi_ewm_ready,
		hmi_ewm_status, hmi_ewm_hplus, hmi_ewm_hminus, hmi_ewm_start, hmi_ewm_enable, hmi_bypass, hmi_stop, hmi_phasing,
		hmi_tns_presence, hmi_ldt_presence, hmi_mode_bit0, hmi_mode_bit1, hmi_mode_bit2, hmi_loop_state, hmi_measured_speed
	}

	partial class PLCReader
	{
		static readonly ushort nregs = 16;
		static readonly byte addr = 1;

		static void Parse(IDictionary<Tag, object> dict, ushort[] regs)
		{
			dict[Tag.hmi_cmd] = GetUInt16(regs, 0); // Код команды
			dict[Tag.hmi_arg0] = GetUInt16(regs, 1); // Аргумент 1
			dict[Tag.hmi_arg1] = GetUInt16(regs, 2); // Аргумент 2
			dict[Tag.hmi_tension] = GetFloat(regs, 4); // Усилие
			dict[Tag.hmi_pos] = GetFloat(regs, 6); // Положение, мм
			dict[Tag.hmi_pos_setpoint] = GetFloat(regs, 8); // Текущая уставка, мм
			dict[Tag.hmi_speed_lim] = GetFloat(regs, 10); // Ограничение скорости, 0 - 1
			dict[Tag.hmi_ewm_ready] = GetBit(regs, 12, 0); // Готовность EWM
			dict[Tag.hmi_ewm_status] = GetBit(regs, 12, 1); // Статус EWM
			dict[Tag.hmi_ewm_hplus] = GetBit(regs, 12, 2); // Не используется
			dict[Tag.hmi_ewm_hminus] = GetBit(regs, 12, 3); // Не используется
			dict[Tag.hmi_ewm_start] = GetBit(regs, 12, 4); // Сигнал пуска EWM
			dict[Tag.hmi_ewm_enable] = GetBit(regs, 12, 5); // Сигнал разрешения работы EWM
			dict[Tag.hmi_bypass] = GetBit(regs, 12, 6); // Состояние байпасного клапана
			dict[Tag.hmi_stop] = GetBit(regs, 12, 7); // Состояние кнопки аварийной остановки
			dict[Tag.hmi_phasing] = GetBit(regs, 12, 8); // Состояние реле контроля фаз
			dict[Tag.hmi_tns_presence] = GetBit(regs, 12, 9); // Присутствие тензодатчика
			dict[Tag.hmi_ldt_presence] = GetBit(regs, 12, 10); // Присутствие датчика линейных перемещений
			dict[Tag.hmi_mode_bit0] = GetBit(regs, 12, 13); // Bit 5
			dict[Tag.hmi_mode_bit1] = GetBit(regs, 12, 14); // Bit 6
			dict[Tag.hmi_mode_bit2] = GetBit(regs, 12, 15); // Bit 7
			dict[Tag.hmi_loop_state] = GetUInt16(regs, 13); // Ограничения по циклу
			dict[Tag.hmi_measured_speed] = GetFloat(regs, 14); // Фактическая скорость
		}
	}
}