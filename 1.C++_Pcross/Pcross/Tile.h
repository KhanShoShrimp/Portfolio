#pragma once

class Tile
{
private :
	int m_Width;
	int m_Height;
	TilePoint* m_Tiles;

public :
	Tile(int width, int height);
	Tile(const TilePoint* data, int width, int height);
	~Tile();
	TilePoint* operator[](int y);
	TileValue GetValueInRow(int y);
	TileValue GetValueInCol(int x);
	void Show();
};