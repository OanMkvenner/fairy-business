#ifndef __ImageProcessingCore__
#define __ImageProcessingCore__

#include <opencv2/features2d/features2d.hpp>
//#include <opencv2/flann/flann.hpp>  //TagJK_FlannCode
//#include <thread>
#include <map>

#define TAG "NativeLib"

using namespace std;
using namespace cv;

const float GOOD_MATCH_PERCENT = 0.15f;
const float GOOD_MATCH_PERCENT_KNN = 0.72f;
static float minimum_match_rating = 0.30f; // minimum workable: probably ~0.30
static int minimum_match_count = 7;		// ~ 25
static float clustering_diameter = 0.031f;//original: 0.037f percent of area allowed for clustiner imprecision
static float bucket_size_multiplier_train = 0.15f; // buckets on training images are bucket_size_multiplier * x-axis of image in width AND height.
static float bucket_size_multiplier_query = 0.05f; // buckets on query images are bucket_size_multiplier * x-axis of image in width AND height.
static int bucketMin_train_flatmin = 7; // minimum number of buckets (but trumped by multipler_max)
static float bucketMin_train_multiplier_min = 0.4f; // special case for way low buckets on train
static int bucketMin_query = 7;

static Mat emptyMat = Mat();
// my Enums always use a namespace to keep Enum usage clean (it forces using DATA_EXCHANGE_TYPE:: in front of any used enum)
namespace DATA_EXCHANGE_TYPE {
	// this is used to specify the type of data that is being sent via "exchangeData()". It has to be used at data1
	enum Enum
	{
		NONE = 0,
		resetCardFilters,
		addCardGroupAllowance,
		addCardGroupDisallowance,
	};
}


namespace FILTER_MODE {
	// this is used to specify the type of data that is being sent via "exchangeData()". It has to be used at data1
	enum Enum
	{
		NONE = 0,	// unused
		AddAllowance,
		AddDisallowance,
		ResetFilters,
	};
}

typedef struct _COMPARISONRETURN
{
	char* resultName;
	float posX;
	float posY;
	float cardRotation;
	float cardScale;
	float matchRating;
	float* keypointDataPtr = nullptr;
	int keypointDataLength = 0;
	int subResultDepth; // 0: a main result, 1: sub result of previous main result, 2: subresult of previous 1st subresult, ...
} COMPARISONRETURN;

typedef struct ComparisonStorage
{
	std::string resultName;
	std::vector<float> keypointData;
} ComparisonStorage;


class ComparisonData
{
	// vars
public:
	int id_original_card = 0; // id of most similar reference pic
	int similarity_original_card = 0; // similarity of most similar reference pic

	float rotation_original_card = 0;
	float scale_original_card = 0;
	Point2f position_original_card = Point2f(0, 0);
	int matches_found = 0;
	int matches_final = 0;
	float match_rating = 0;

	std::vector<DMatch> matches = std::vector<DMatch>(); // only used in debug to return and draw best matches

	//uniqueness-data
	float uniqueness = 0; // defines how dissimilar this cards original is relative to all other cards
	int cards_above_threshhold = 0; // amount of cards that are definitely too similar to this card (above accepted similarity threshhold)
	int id_most_similar_to_original = 0; // id of card, thats most similar to this cards original
	float similarity_most_similar_to_original = 0; // similarity of card, thats most similar to this cards original

	std::vector<ComparisonData> subResults;

	ComparisonData() {};
	~ComparisonData() {};
	bool operator > (const ComparisonData& secon_obj) const
	{
		return (similarity_most_similar_to_original > secon_obj.similarity_most_similar_to_original);
	}
};

class FolderSettings
{
	// vars
public:
	int scanningResolution = 0; // resolution used for scanning the content of the folder(s)
	int generationResolution = 0; // resolution used during keypoint generation for the folders content

	FolderSettings() {};
	~FolderSettings() {};
};


class RotationMatch
{
	// vars
public:
	float rot; // rotation
	size_t match_id; // id of corresponding match
	RotationMatch(float rotation, size_t matchID) {
		rot = rotation;
		match_id = matchID;
	};
	~RotationMatch() {};
	bool operator < (const RotationMatch& secon_obj) const
	{
		return (rot < secon_obj.rot);
	}
};

// references another keypoint of another card, to wich this keypoint is similar
class SimilarKeypoint
{
public:
	size_t idSimilarKeypointSet; // keypointset (card/image) to wich this keypoint is similar
	size_t keypoint_id; // keypoint of above keypointset, to wich this keypoint is similar
	SimilarKeypoint(size_t i_idSimilarKeypointSet, size_t i_keypoint_id) {
		idSimilarKeypointSet = i_idSimilarKeypointSet;
		keypoint_id = i_keypoint_id;
	};
	~SimilarKeypoint() {};
	bool operator == (const SimilarKeypoint& secon_obj) const
	{
		return (idSimilarKeypointSet == secon_obj.idSimilarKeypointSet && keypoint_id == secon_obj.keypoint_id);
	}
};

class RecentScan
{
public:
	unsigned int cardIdx = 65535; // 65535 == invalid
	unsigned int lastValidScanIteration = 0;
};

namespace cv
{
	class CLAHE;
}
class ImageComparator
{
// vars
private:
	Ptr<DescriptorMatcher> mMatcher;
	//TagJK_FlannCode
	//cv::Ptr<cvflann::LinearIndexParams> params3;
	//cv::Ptr<cvflann::LshIndexParams> params2;
	//cv::Ptr<cvflann::HierarchicalClusteringIndexParams> params;
	//std::vector<Ptr<cv::flann::GenericIndex<cvflann::HammingLUT>>> mFlannIndex;
	Ptr<cv::Feature2D> mOrbInitial;
public: std::vector<cv::Mat> mPics;//just for debugging
	cv::Mat emptyPic = cv::Mat();// for returning empty pic
	std::vector<std::vector<cv::KeyPoint>> mKeypoints;
	std::vector<std::vector<std::vector<SimilarKeypoint>>> mKeypointSimilarities;
	std::vector<std::vector<unsigned int>> mCardSimilarities; // each card has a list of cards that NEED to be checked as well before allowing early cancel
	std::vector<unsigned int> mPartialDetectionKeypointlist; // if only a few keypoints are detected, but the picture they belong is the 
															 // same in subsequent checks, we gather all found keypoints until we have a definite resolution
	unsigned int scanIteration = 0; // just iterates upwards each frame - returns to 0 when overflowing
	int mShiftingSubsetIndex = 0; // this shifts through the active cards, only scanning X cards per call and returning to start once the whole set was scanned
	std::vector<cv::Mat> mDescriptors;
	std::vector<std::vector<cv::KeyPoint>> mBackupKeypoints;
	std::vector<cv::Mat> mBackupDescriptors;
	std::vector<cv::Point2f> mPicCenters;
	std::vector<int> mRequiredBuckets; // shows how many of the "buckets" (spaces) of the training image need to be filled with keypoints
	cv::Mat standstillData;
	cv::UMat lastPic;
	cv::Mat gammaLookupTable;
	cv::Mat searchMask;
	bool reverseIterationDir = false;
	// filters
	std::vector<std::string> groupNames; // only used as lookup table for basic long-number based "exchangeData()" data exchange
	std::map<string, std::vector<unsigned int>> cardIndexGroups; // used to ex-/include only certain cards into matching (card index)
	std::map<string, std::vector<std::string>> childrenGroups; // used to do multi-component scanning. defines if this group has child groups, and which these are
	std::map<string, bool> cardGroupEarlyCancelAllowance; // each cardGroup has an allowance bool, that specifies if early checking-breakout it allowed. 
																		 //  On Groups that are too similar to each other, early check cancel can lead to incorrect results!
	std::map<string, RecentScan> recentResults; // used to do multi-component scanning
	std::vector<std::string> originGroups;
	std::vector<unsigned int> activeCards; // a vector of indices, pointing to all currently allowed cards. Refreshed on every filter-change
	bool earlyCancelOnActiveCards = false; // specifies if currently active cards allow early check-canceling
	std::vector<std::string> disallowedCardGroupsFilter;  // only cards that are NOT part of any of these groups are allowed
	std::vector<std::string> allowedCardGroupsFilter; // if this has one or more entries, only cards fitting in at least one of the groups are allowed
public:
	std::vector<COMPARISONRETURN> returnContent;
	std::vector<ComparisonStorage> returnContentStorage;
	std::vector<KeyPoint> storedQueryKeypoints;
	std::vector <ComparisonData> m_data = std::vector<ComparisonData>();

	float m_currPerspectiveMult = 0.5; // value between 0.0 and 1.0, indicating angle of camera perspective (0 would be along the table, 1 would be watching straight from atop)
	float m_perspectiveConfidence = 0; // increases if current perspective is reinforced, reduces if we cant find anything on current perspective or if perspective changes
	int m_perspectiveCheckDirection = -1; // flipflops between +1 and -1
	int m_timesSinceLastPerspectiveCheck = 0;

	std::vector<std::string> mFilenames;
	std::string mResultFileName; // used to return a somewhat stable adress of a returnstring
	const std::string noFileFoundName = ""; // this ones reference is returned in case no similar file is found
	const std::string noFilesLoadedYet = "no_files_loaded_yet"; // this ones reference is returned in case no files have been found to compare with
	Ptr<cv::Feature2D> mBADDescriptor; // descriptor for keypoints that are found by ORB. "BAD" will be available in OpenCV 4.7 under the name "TEBID" in experimental 
	Ptr<cv::Feature2D> mOrbCompare;
	bool increaseBrightness; // this is set to true for every second picture
	bool usePreprocessing;
	bool useSharpening;
	bool debugFlag;
	float debugValue;
	float gammaCorrectionFactor;
	float searchMaskSize;
	float scanResolution;
	bool positionSameAsFirst = true;
	bool rotationSameAsFirst = true;
	bool scaleSameAsFirst = true;
	int customKpLevels;
	Ptr<cv::CLAHE> claheHistogramEqualizer;
	cv::Point2i currentPictureSize = cv::Point2i(1,1);

	string addNextImageWithName = "";
	bool addNextImageWithNameincludePic = false;

	void updateEnabledKeypoints(size_t card_id, const cv::Mat& disabled_areas);
	//functions:
public:
	bool checkRotationSimilar(float rot1, float rot2);
	bool checkScaleSimilar(float scale1, float scale2);
	int checkFilledBuckets(cv::Point2f cardSize, float bucketSizeMultiplier, std::vector<Point2f> keypointPoints, int earlyCancelValue = 99999);
	bool compareMatchKeyppointResolution(DMatch& a, DMatch& b);
	void initFromLocalPictures();
	void addNextPictureToScannables(string fileName, bool includingPicture);
	void addImageWithPrimedName(Mat& pic);
	int initFromFile(const char* utfString);
	void safeLocalDescriptors();
	void initSettingsFromFile(const char* utfString);
	void initScanningResolution(FileStorage& fs);
	void writeScanningResolution(FileStorage& fs);
	void checkUniquenessOfAll();
	size_t initFromPictures(const char* pathUtf, char* allFiles[], int amountOfFiles, bool safePicData);
	size_t initFromPicturesWithGroups(string path, std::vector<string> allFiles, std::vector<string> allFileNames, std::string groupname, std::vector<std::string> childGroups, bool safePicData);
	void readFileColorsAndAlphaMask(const String& finalPath, std::vector<Mat>& pics, std::vector<Mat>& masks);
	void initPicturesOnly(string path, std::vector<string> allFiles, std::string groupname, std::vector<std::string> childGroups);
	size_t addSinglePictureWithoutGroup(cv::Mat& pic, string fileName, bool safePicData);
	void calculateRequiredBuckets(std::vector<KeyPoint>& keypoints);
	bool removeScannableByName(std::string name);
	bool removeScannableFromBack();
	bool removeScannableByIndex(unsigned int cardIndex);
	bool initWithPictureList(std::vector<cv::Mat>& pics, std::vector<cv::Mat>& keypointMasks, bool safePicData);
	void addName(std::string name);
	bool initDebugPicturesOnly(std::vector<cv::Mat>& pics);
	void setGammaCorrection(float gamma_correction_level /*lower == brighter, 0.01-1.0*/);
	void initGammaLookupTable(float gamma_correction_level);
	void setCustomKpLevels(int value);
	void setScanResolution(float value);
	void setSearchMaskRadiusPercent(float radiusPercentOfWidth);
	void initSearchMask(int width, int height, float radiusPercentOfWidth);
	void preprocessPicture(cv::Mat& pic, bool initializingCard, cv::Mat& mask = emptyMat);
	void splitPictureIntoSectors(cv::Mat& pic, cv::Mat& mask = emptyMat);
	void preprocessPictureRemoveStaticBackground(cv::Mat &pic);
	bool wasRecentlyScanned(RecentScan recentScan);
	void iterativeGroupBasedDetection(std::vector<std::string>& targetGroupNames, std::vector<KeyPoint>& keypoints1, Mat& descriptors1, double resolutionDiagonal, std::vector<ComparisonData>& dataVec, bool innerGroupTest = false, bool singlePictureOnly = false);
	void detectAndComputeSpreadOut(Ptr<cv::Feature2D> orbGenerator, const cv::Mat& pic, InputArray mask, std::vector<KeyPoint>& query_keypointsFinal, OutputArray descriptors, int optionalMaxFeatureOverride = -1);
	void detectAndComputeSpreadOutUsingBuckets(Ptr<cv::Feature2D> orbGenerator, const cv::Mat& pic, InputArray mask, std::vector<KeyPoint>& query_keypointsFinal, OutputArray descriptors, int initialFeatureCount);
	void registerNewScan();
	void PostprocessDataVec(std::vector<ComparisonData>& dataVec, int finalPicWidth, int finalPicHeight);
	std::vector<KeyPoint> compareImageDebug(cv::Mat& pic, std::vector<ComparisonData>& dataVec, bool singlePictureOnly = false);
	std::vector<KeyPoint> compareImageBoard(cv::Mat& pic, std::vector<ComparisonData>& dataVec, bool singlePictureOnly = false, bool debugImageOutput = false);
	void findClosestMatchFast(
		std::vector<unsigned int>& indicesAlreadyChecked,
		std::vector<unsigned int> indicesToCheck,
		bool cancelOnFirstMatch,
		// data for "matchResults()" 
		std::vector<KeyPoint>& keypoints1 /*query pic, the one newly scanned*/,
		Mat& descriptors1,
		double minimum_diameter,
		int minimumMatches,
		int firstCheckFastCancel, // if this is anything but "-1" and we get a result on the first check, we cancel immideately! 
		std::vector<ComparisonData>& dataVec);
	cv::Mat checkUniqueness(ComparisonData& data, bool draw_result = true);
	const void checkUniqueKeypoints(std::vector<unsigned int> &indicesToCheckAgainstEachother);
	void adaptiveThreshold(cv::Mat& mat);
	const void removePerspective(cv::Mat& mat, float heightMult, float widthMult);
	//const std::string& imageDetection(cv::UMat &mat);
	const std::string& imageDetection(cv::Mat& mat);
	const std::string& testGrayOutPic(cv::Mat& mat);
	const std::string& detectSingleImageString(cv::Mat& mat, bool isColoredPicture, bool debugImageOutput = false);
	void detectSingleImage(cv::Mat& mat, bool isColoredPicture, bool debugImageOutput = false);
	const void boardDetection(cv::Mat& mat, float heightMult, bool debugImageOutput);
	long exchangeData(long dataType, long data);
	long callCommand(std::string commandType, std::string value);
	const std::vector<std::vector<cv::KeyPoint>>& getKeypointVectors();
	const std::vector<cv::Mat>& getPictures();
	const cv::Mat& getPictureOfIndex(unsigned int index);
	bool hasKeypointBackup();
	void createKeypointBackup();
private:
	void matchResults(const std::vector<cv::KeyPoint>& keypoints1, const cv::Mat& descriptors1, const std::vector<cv::KeyPoint>& keypoints2, const cv::Mat& descriptors2, int minimumMatches, Point2f middle_of_reference_card, std::vector<ComparisonData>& data, int idxComparePic, double resolutionDiagonal, bool useALLMatches = false);
	int findAreaSpreadBucket(cv::Point2f cardSize, cv::Point2f point, float bucketSizeMultiplier);
	void recalculateOriginGroups();
	void addCardToGroup(string groupName, string cardName, unsigned int cardIndex);
	void deleteScannableFromAllGroupsByIndex(unsigned int cardIndex);
	void addCardGroup(std::string groupName);
	size_t addCardGroup(std::string groupName, std::vector<unsigned int> cardIndices, std::vector<std::string> childGroups, bool allowCardGroupEarlyCancel);
	void clearAllCardGroups();
	bool setCardFilter(FILTER_MODE::Enum filter, std::string groupName = "");
	void recalculateFilteredCards();
	void createHardcodedDolphinGroups();

public:
	ImageComparator();
	~ImageComparator();
};

void imageDetectionMat(cv::Mat &mat);




class MeanShift {
public:
	MeanShift() {}
	// points: 2d float points of clustering data; 
	// kernel_bandwidth: approx. radius for meanShift kernel; 
	// EPSILON: minimum movement below wich to stop the algorithm
	// minimumPointsPerCluster: when a cluster stops moving its center, disregard it if it has less points than this.
	static std::vector<Point2f> meanshift(const std::vector<Point2f>& points, float kernel_bandwidth, float EPSILON, int minimumPointsPerCluster);

private:
	static int shift_point(const Point2f&, const std::vector<Point2f>&, float, Point2f&);
};


#endif /* defined(__ImageProcessingCore__) */
