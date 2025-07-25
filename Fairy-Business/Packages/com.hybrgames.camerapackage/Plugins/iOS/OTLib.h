#pragma once
#include "Core.h"

#ifdef _WINDLL
#define DllExport __declspec( dllexport )
#else
#define DllExport 
#endif

struct Color32
{
	unsigned char red;
	unsigned char green;
	unsigned char blue;
	unsigned char alpha;
};

extern "C"
{
	DllExport float Foopluginmethod();
	DllExport void* createImageComparator();
	DllExport void deleteImageComparator(void* &comparator_ptr);
	DllExport const char * findImage(void* comparator_ptr, void**rawImage, int width, int height, bool isColoredPicture, bool debugImageOutput);
	DllExport const void findGameBoard(void* comparator_ptr, void** rawImage, int width, int height, int cameraAngle, float heightMult, COMPARISONRETURN** returnStructs, int* returnArraySize, bool debugImageOutput);
	DllExport const void findImageWithReturnstructs(void* comparator_ptr, void** rawImage, int width, int height, COMPARISONRETURN** returnStructs, int* returnArraySize, bool isFlippedHorizontally, bool isColoredPicture, bool debugImageOutput);
	DllExport int initializeImages(void* comparator_ptr, char* file_path);
	DllExport void setCustomValues(void* comparator_ptr, char* valueTypeUtfString, float value1, float value2);
	DllExport long callCommand(void* comparator_ptr, char* commandTypeUtfString, char* value);
	DllExport float testImageComparatorCreation(void* comparator_ptr);
	DllExport int initializeImagesDirectly(void* comparator_ptr, char* file_path, char* allFiles[], int amountOfFiles);
}