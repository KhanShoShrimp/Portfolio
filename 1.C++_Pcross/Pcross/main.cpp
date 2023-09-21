#include "Base.h"
#include <conio.h>

const int WIDTH = 10;
const int HEIGHT = 10;

void Show(Tile& select, Tile& block)
{
	cout << "Map : " << endl;
	for (int y = 0; y < HEIGHT; y++)
	{
		for (int x = 0; x < WIDTH; x++)
		{
			if (select[y][x])
			{
				cout << "■";
			}
			else if (block[y][x])
			{
				cout << "Ⅹ";
			}
			else
			{
				cout << "□";
			}
		}
		cout << endl;
	}
}

void SolvePuzzle(Tile& select, Tile& block, 
	int* col_answer, int* col_answer_length,
	int* row_answer, int* row_answer_length)
{
	int x, y, offset;

	for (size_t i = 0; i < 3; i++)
	{
		offset = 0;
		for (y = 0; y < HEIGHT; y++)
		{
			Solution solution(
				select.GetValueInRow(y),
				block.GetValueInRow(y),
				WIDTH,
				row_answer + offset,
				row_answer_length[y]);

			offset += row_answer_length[y];

			for (x = 0; x < WIDTH; x++)
			{
				if ((solution.SelectHint >> x) & 1)
				{
					select[y][x] = true;
				}
				if ((solution.CancelHint >> x) & 1)
				{
					block[y][x] = true;
				}
			}
		}

		offset = 0;
		for (x = 0; x < WIDTH; x++)
		{
			Solution solution(
				select.GetValueInCol(x),
				block.GetValueInCol(x),
				HEIGHT,
				col_answer + offset,
				col_answer_length[x]);

			offset += col_answer_length[x];

			for (y = 0; y < HEIGHT; y++)
			{
				if ((solution.SelectHint >> y) & 1)
				{
					select[y][x] = true;
				}
				if ((solution.CancelHint >> y) & 1)
				{
					block[y][x] = true;
				}
			}
		}
	}
}

int main()
{
	//Example Map
	int col_answers[] = { 1,8,9,2,3,2,6,2,5,2,1,3,6,5,4 };
	int col_answer_length[] = { 1,1,1,3,2,2,2,1,1,1 };
	int row_answers[] = { 3,5,3,2,5,5,3,9,2,3,2,4,8,5 };
	int row_answer_length[] = { 1,1,2,1,2,1,2,2,1,1 };

	Tile select(WIDTH, HEIGHT);
	Tile block(WIDTH, HEIGHT);

	SolvePuzzle(select, block, col_answers, col_answer_length, row_answers, row_answer_length);
	Show(select, block);
}

