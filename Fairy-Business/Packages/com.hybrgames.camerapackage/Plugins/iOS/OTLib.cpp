#include "OTLib.h"


//extern "C" {
//	/* This trivial function returns the platform ABI for which this dynamic native library is compiled.*/
//	const char * SharedObject2::getPlatformABI()
//	{
//	#if defined(__arm__)
//	#if defined(__ARM_ARCH_7A__)
//	#if defined(__ARM_NEON__)
//		#define ABI "armeabi-v7a/NEON"
//	#else
//		#define ABI "armeabi-v7a"
//	#endif
//	#else
//		#define ABI "armeabi"
//	#endif
//	#elif defined(__i386__)
//		#define ABI "x86"
//	#else
//		#define ABI "unknown"
//	#endif
//		return "This native library is compiled with ABI: %s" ABI ".";
//	}
//
//	void SharedObject2()
//	{
//	}
//
//	SharedObject2::SharedObject2()
//	{
//	}
//
//	SharedObject2::~SharedObject2()
//	{
//	}
//}

extern "C"
{
	DllExport float /*SharedObject1::*/Foopluginmethod()
	{
		cv::Mat img(10, 10, CV_8UC1); // use some OpenCV objects
		return 1;// img.rows * 1.0f;     // should return 10.0f
	}
	DllExport void * createImageComparator()
	{
		return new ImageComparator;
	}

	DllExport void deleteImageComparator(void* &comparator_ptr)
	{
		ImageComparator* comparator = static_cast<ImageComparator *>(comparator_ptr);
		delete comparator;
		comparator_ptr = nullptr;
	}

	DllExport void setCustomValues(void* comparator_ptr, char* valueTypeUtfString, float value1, float value2) {
		ImageComparator* comparator = static_cast<ImageComparator*>(comparator_ptr);
		std::string valueType(valueTypeUtfString);
		if (valueType == "setSearchMaskRadiusPercent")
		{
			comparator->setSearchMaskRadiusPercent(value1);
		}
		if (valueType == "setScanResolution")
		{
			comparator->setScanResolution(value1);
		}

	}

	DllExport long callCommand(void* comparator_ptr, char* commandTypeUtfString, char* value) {
		ImageComparator* comparator = static_cast<ImageComparator*>(comparator_ptr);
		std::string commandType(commandTypeUtfString);
		std::string commandValue(value);
		return comparator->callCommand(commandType, commandValue);
	}

	DllExport const char* findImage(void* comparator_ptr, void** rawImage, int width, int height, bool isColoredPicture, bool debugImageOutput)
	{
		ImageComparator* comparator = static_cast<ImageComparator*>(comparator_ptr);

		// create an opencv object sharing the same data space
		Mat mat(height, width, CV_8UC4, *rawImage);
		Mat safetyCopy;
		// if debugging: dont work with original but work with a copy of the original, and copy it back into original later (needs to be same dimensions at copytime!!!)
		if (debugImageOutput) {
			safetyCopy = mat;
			mat = Mat(height, width, CV_8UC4);
			safetyCopy.copyTo(mat);
		}

#if defined(_WIN32) or defined(_WIN64)
		//images are flipped in windows camera!
		flip(mat, mat, 1);
#endif
		const std::string& ret = comparator->detectSingleImageString(mat, isColoredPicture, debugImageOutput);
		//const std::string& ret = comparator->testGrayOutPic(mat);
		if (debugImageOutput)
		{
			// merge changes on the mat into the actual *rawImage ( "mat" has a different imagereference because of RGB/GRAY type-changes)
			// Needs to be same dimensions at copytime for this to work!!! otherwise different memory is allocated
			mat.copyTo(safetyCopy);
		}
		return ret.c_str();
	}

	void clearOldSearchResults(ImageComparator* comparator) {
		comparator->returnContent.clear();
		comparator->returnContentStorage.clear();
	}
	void addSearchResult(ImageComparator* comparator, ComparisonData& data, int subResultDepth, bool returnKeypointsAsWell, bool isFlippedHorizontally) {
		comparator->returnContent.push_back(COMPARISONRETURN());
		comparator->returnContentStorage.push_back(ComparisonStorage());
		COMPARISONRETURN& returnItem = comparator->returnContent.back();
		ComparisonStorage& returnStorage = comparator->returnContentStorage.back();
		//std::string name = comparator->mFilenames[data.id_original_card];
		Point2f pos = data.position_original_card;
		float rotation = data.rotation_original_card;
		if (isFlippedHorizontally)
		{
			rotation = -rotation;
		} else {
			pos.x = 1.0f - pos.x;
		}

		// ****** WARNING!!! KEYPOINT STORAGE DOESNT CURRENTLY WORK FOR MULTI-SCANS, or even for normal ones maybe. ******
		// Since scanning now happens on parallel sometimes, returnContentStorage gets removed too quickly sometimes!
		returnStorage.keypointData = std::vector<float>();
		if (returnKeypointsAsWell)
		{
			returnStorage.keypointData.reserve(data.matches.size() * 3);
			for (auto match : data.matches)
			{
				int i_query = match.queryIdx; // newly scanned picture keypoints
				const KeyPoint& q_keypoint = comparator->storedQueryKeypoints[i_query];

				returnStorage.keypointData.push_back(q_keypoint.pt.x);
				returnStorage.keypointData.push_back(q_keypoint.pt.y);
				returnStorage.keypointData.push_back(q_keypoint.size);

				// get color depending on Train-picture index and use that for the whole line
				//int t_color_id = (i_train % color_id_modulo_minus_one) + 1;
				// color of this query keypoint should be the same as its matched train keypoint
				//special_query_colors[i_query] = t_color_id;
			}
		}
		// save values in local storage
		//returnStorage.resultName = name;
		// add data or references to returned item data

		returnItem.posX = pos.x;
		returnItem.posY = pos.y;//pos.y;
		returnItem.matchRating = data.match_rating;
		returnItem.cardRotation = rotation;
		returnItem.cardScale = data.scale_original_card;
		returnItem.resultName = (char*)comparator->mFilenames[data.id_original_card].c_str();
		returnItem.keypointDataLength = returnStorage.keypointData.size();
		if (returnStorage.keypointData.size() > 0)
		{
			returnItem.keypointDataPtr = &returnStorage.keypointData[0];
		} else {
			returnItem.keypointDataPtr = nullptr;
		}
		returnItem.subResultDepth = subResultDepth;
		// handle subResults
		for (auto subResultData : data.subResults)
		{
			addSearchResult(comparator, subResultData, subResultDepth + 1, returnKeypointsAsWell, isFlippedHorizontally);
		}
	}
	void addSearchResults(ImageComparator* comparator, bool returnKeypointsAsWell, bool isFlippedHorizontally) {
		for (ComparisonData& data : comparator->m_data)
		{
			addSearchResult(comparator, data, 0, returnKeypointsAsWell, isFlippedHorizontally);
		}
	}

	DllExport const void findGameBoard(void* comparator_ptr, void** rawImage, int width, int height, int cameraAngle, float heightMult, COMPARISONRETURN** returnStructs, int* returnArraySize, bool debugImageOutput)
	{
		ImageComparator* comparator = static_cast<ImageComparator*>(comparator_ptr);

		// ********************************* ADD isFlippedHorizontally TO INPUT! ****************************************************************

		comparator->increaseBrightness = false; // this was originally supposed to be set to true for every second picture
		comparator->usePreprocessing = true; // default: true
		comparator->useSharpening = true; // default: false
		// create an opencv object sharing the same data space
		Mat mat(height, width, CV_8UC4, *rawImage);

		// need to rotate it as per the cameraAngle AND mirror the view horizontally; so up is actually up and left and right are not mirrored
		if (cameraAngle == 90)
		{
			//#if defined(_WIN32) or defined(_WIN64) //images are flipped in windows camera!
			transpose(mat, mat);
		}
		else if (cameraAngle == 180)
		{
			//#if defined(_WIN32) or defined(_WIN64) //images are flipped in windows camera!
			flip(mat, mat, -1);
		}
		else if (cameraAngle == 270)
		{
			//#if defined(_WIN32) or defined(_WIN64) //images are flipped in windows camera!
			transpose(mat, mat);
			flip(mat, mat, -1);
		}
		else
		{
			// cameraAngle == 0 assumed. other angles are not handled
#if defined(_WIN32) or defined(_WIN64) 
			//images are flipped in windows camera!
			flip(mat, mat, 1);
#endif
		}

		//using namespace cv;
		//cv::UMat umat = mat.getUMat(cv::ACCESS_RW);
		comparator->boardDetection(mat, heightMult, debugImageOutput);
		if (debugImageOutput)
		{
			// merge changes on the mat into the actual *rawImage ( "mat" has a different imagereference because of RGB/GRAY type-changes)
			Mat mat_orig(height, width, CV_8UC4, *rawImage);
			mat.copyTo(mat_orig);
		}

		clearOldSearchResults(comparator);
		bool isFlippedHorizontally = false;
		addSearchResults(comparator, true, isFlippedHorizontally);
		*returnStructs = &comparator->returnContent[0];
		*returnArraySize = comparator->returnContent.size();

		//COMPARISONRETURN* pCur = *returnStructs;
		//size_t len;
		//for (int i = 0; i < returnArraySize; i++)
		//{
		//	pCur->posX = 5;
		//	*pCur = comparator->returnContent.back();
		//	pCur++;
		//}

	}

	DllExport const void findImageWithReturnstructs(void* comparator_ptr, void** rawImage, int width, int height, COMPARISONRETURN** returnStructs, int* returnArraySize, bool isFlippedHorizontally, bool isColoredPicture, bool debugImageOutput)
	{
		ImageComparator* comparator = static_cast<ImageComparator*>(comparator_ptr);

		comparator->increaseBrightness = false; // this was originally supposed to be set to true for every second picture
		comparator->usePreprocessing = true; // default: true
		comparator->useSharpening = true; // default: false
		// create an opencv object sharing the same data space
		Mat mat(height, width, CV_8UC4, *rawImage);


		//images are flipped in windows camera!
		if (isFlippedHorizontally) {
			flip(mat, mat, 1);
		}

		//using namespace cv;
		//cv::UMat umat = mat.getUMat(cv::ACCESS_RW);
		comparator->detectSingleImage(mat, isColoredPicture, debugImageOutput);
		if (debugImageOutput)
		{
			// merge changes on the mat into the actual *rawImage ( "mat" has a different imagereference because of RGB/GRAY type-changes)
			Mat mat_orig(height, width, CV_8UC4, *rawImage);
			mat.copyTo(mat_orig);
		}

		// generate results for library return
		clearOldSearchResults(comparator);
		addSearchResults(comparator, false, isFlippedHorizontally);
		*returnStructs = &comparator->returnContent[0];
		*returnArraySize = comparator->returnContent.size();
	}

	// init from a prepared keypointfile
	DllExport int initializeImages(void* comparator_ptr, char* file_path)
	{
		ImageComparator* comparator = static_cast<ImageComparator *>(comparator_ptr);
		return comparator->initFromFile(file_path);
	}

	// init from a list of image paths
	DllExport int initializeImagesDirectly(void* comparator_ptr, char* file_path, char * allFiles[], int amountOfFiles)
	{
		ImageComparator* comparator = static_cast<ImageComparator *>(comparator_ptr);
		return comparator->initFromPictures(file_path, allFiles, amountOfFiles, false);
	}

	DllExport float testImageComparatorCreation(void* comparator_ptr)
	{
		ImageComparator* comparator = static_cast<ImageComparator *>(comparator_ptr);
		return comparator->mFilenames.size();
	}
}
