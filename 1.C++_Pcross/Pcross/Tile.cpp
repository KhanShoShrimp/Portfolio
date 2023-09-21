#include "Base.h"

Tile::Tile(int width, int height)
{
	if (width < 1 || width > 63 || height < 1 || height > 63)
	{
		throw std::out_of_range("width and height must be between 1 and 63");
	}
	m_Width = width;
	m_Height = height;
	m_Tiles = new TilePoint[width * height]{ false };
}

Tile::Tile(const TilePoint* data, int width, int height) : Tile(width, height)
{
	std::memcpy(m_Tiles, data, width * height);
}

Tile::~Tile()
{
	delete[] m_Tiles;
}

bool* Tile::operator[](int y)
{
	if (y < 0 || y >= m_Height)
	{
		throw std::out_of_range("Index out of range");
	}
	return m_Tiles + y * m_Width;
}

TileValue Tile::GetValueInRow(int y)
{
	TileValue value = 0;
	for (int x = 0; x < m_Width; x++)
	{
		if (m_Tiles[y * m_Width + x])
		{
			value |= (TileValue(1) << x);
		}
	}
	return value;
}

TileValue Tile::GetValueInCol(int x)
{
	int64_t value = 0;
	for (int y = 0; y < m_Height; y++)
	{
		if (m_Tiles[y * m_Width + x])
		{
			value |= (int64_t(1) << y);
		}
	}
	return value;
}

void Tile::Show()
{
	for (int y = 0; y < m_Height; y++)
	{
		for (int x = 0; x < m_Width; x++)
		{
			if ((*this)[y][x])
			{
				cout << "бс";
			}
			else
			{
				cout << "бр";
			}
		}
		cout << endl;
	}
}