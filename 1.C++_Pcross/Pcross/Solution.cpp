#include "Base.h"

Solution::Solution(TileValue select, TileValue block, int length, int * answer, int count)
{
	m_Answer = answer;
	m_Answer_Length = count;

	if (!IsValidAnswers())
	{
		return;
	}

	m_Tile_Block = block | PLUSTABLE[length];
	m_Tile_Select = select;
	m_Tile_length = length;

	SelectHint = FILL;
	CancelHint = EMPTY;

	int start = 0, end = 0;
	while (FindSpace(start, end))
	{
		m_Spaces.push_back(start);
		m_Spaces.push_back(end);
	}

	Solve(0, 0, ~MINUSTABLE[length - 1], EMPTY);

	SelectHint = (SelectHint ^ m_Tile_Select);
	CancelHint = (~CancelHint ^ m_Tile_Block);
}

bool Solution::IsValidAnswers()
{
	return m_Answer_Length > 0 && m_Answer[0] != 0;
}

bool Solution::FindSpace(int& start, int& end)
{
	if (!FFSLL(~m_Tile_Block & PLUSTABLE[end], start))
	{
		return false;
	}
	if (!FFSLL(m_Tile_Block & PLUSTABLE[start], end))
	{
		return false;
	}
	return true;
}

void Solution::Solve(
	int space_index, int answer_start, 
	Int64 result_select, Int64 result_cancel)
{
	if (answer_start >= m_Answer_Length)
	{
		SelectHint &= result_select;
		CancelHint |= result_cancel;
	}
	else
	{
		for (int i = space_index; i < m_Spaces.size(); i += 2)
		{
			int space = m_Spaces[i + 1] - m_Spaces[i];
			Int64 bitmask = MINUSTABLE[space - 1] << m_Spaces[i];

			for (int j = answer_start; j < m_Answer_Length; j++)
			{
				if (CheckSpacing(space, answer_start, j))
				{
					Int64 select = bitmask;
					Int64 cancel = 0;

					GetOverlapedTile(
						m_Spaces[i], 
						m_Spaces[i + 1], 
						answer_start, 
						j,
						EMPTY,
						bitmask,
						select,
						cancel);

					Solve(i + 2, j + 1, (result_select | select) & ~ m_Tile_Block, result_cancel | cancel);
				}
			}

			if ((bitmask & m_Tile_Select) != 0)
			{
				break;
			}
		}
	}
}

bool Solution::CheckSpacing(int space, int answer_start, int answer_end)
{
	//return AnswerTotalSize(answer_start, answer_end) <= space;

	for (int i = answer_start; i < answer_end; i++)
	{
		if (m_Answer[i] + 1 <= space)
		{
			space -= m_Answer[i] + 1;
		}
		else
		{
			return false;
		}
	}
	return m_Answer[answer_end] <= space;
}

void Solution::GetOverlapedTile(
	int space_index, const int& space_end,
	int answer_index, const int& answer_end,
	Int64 crnt, const Int64& bitmask,
	Int64& result_select, Int64& result_cancel)
{
	int totalsize = AnswerTotalSize(answer_index, answer_end);
	int end = space_end - totalsize;

	if (space_index > end)
	{
		return;
	}

	if (answer_index < answer_end)
	{
		for (int i = space_index; i <= end; i++)
		{
			Int64 temp = MINUSTABLE[m_Answer[answer_index] - 1] << i;

			GetOverlapedTile(
				i + m_Answer[answer_index] + 1,
				space_end,
				answer_index + 1,
				answer_end,
				crnt | temp,
				bitmask,
				result_select,
				result_cancel);
		}
	}
	else
	{
		for (int i = space_index; i <= end; i++)
		{
			Int64 temp = MINUSTABLE[m_Answer[answer_index] - 1] << i;
			Int64 result = crnt | temp;

			if ((m_Tile_Select & ~result & bitmask) == 0)
			{
				result_select &= result;
				result_cancel |= result;
			}
		}
	}
}

int Solution::AnswerTotalSize(int answer_start, int answer_end)
{
	int totalsize = 0;
	for (int i = answer_start; i < answer_end - 1; i++)
	{
		totalsize += m_Answer[i] + 1;
	}
	return totalsize + m_Answer[answer_end];
}

bool Solution::FFSLL(Int64 value, int& index)
{
	int num = 0;
	if (!value)
		return false;
	if ((value & 0xffffffffULL) == 0) {
		num += 32;
		value >>= 32;
	}
	if ((value & 0xffffULL) == 0) {
		num += 16;
		value >>= 16;
	}
	if ((value & 0xffULL) == 0) {
		num += 8;
		value >>= 8;
	}
	if ((value & 0xfULL) == 0) {
		num += 4;
		value >>= 4;
	}
	if ((value & 0x3ULL) == 0) {
		num += 2;
		value >>= 2;
	}
	if ((value & 0x1ULL) == 0)
		num += 1;

	index = num;
	return true;
}

bool Solution::FLSLL(Int64 value, int& index)
{
	int num = 63;
	if (!value)
		return false;

	if (!(value & 0xffffffff00000000ULL)) {
		value <<= 32;
		num -= 32;
	}
	if (!(value & 0xffff000000000000ULL)) {
		value <<= 16;
		num -= 16;
	}
	if (!(value & 0xff00000000000000ULL)) {
		value <<= 8;
		num -= 8;
	}
	if (!(value & 0xf000000000000000ULL)) {
		value <<= 4;
		num -= 4;
	}
	if (!(value & 0xc000000000000000ULL)) {
		value <<= 2;
		num -= 2;
	}
	if (!(value & 0x8000000000000000ULL)) {
		value <<= 1;
		num -= 1;
	}
	index = num;
	return true;
}