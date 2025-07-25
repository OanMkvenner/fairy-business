#pragma warning(disable:4996)
//#include <string.h>
#include "Core.h"
//#include <opencv2/features2d.hpp>
//#include <algorithm>
#include <random>
//#include "opencv2/imgproc/imgproc.hpp"
#include "HighPerfUtilities.h"

//#include <opencv2/highgui/highgui.hpp>

//#include <iostream>
//#include <fstream>

using namespace std;
using namespace cv;

#define PI 3.14159265

// Atlantis: 500, Thargos: 600, houston: 400 Scanning Demonstration: 600, PeaceAndWar 350
double STANDARDIZED_RESOLUTION = 400;// changes resolution of scanned picture. Bigger == finer resoltution && FURTHER card distance
// Atlantis: 400, Thargos: 400, houston: 400 , Scanning Demonstration: 400
double KEYPOINT_GENEREATION_RESOLUTION = 400;// changes resolution of reference cards. Bigger == finer resoltuion  &&  CLOSER card distance

namespace staticNamespace {
	static float spreadOutMultiplier = 10.0; // (value == 1: no spreading!) used to spread keypoints into weaker areas as well. Creates x times keypoints and then filters out randomly to go back to original keypoint count
	
	// Atlantis: 450, Thargos: 450, houston: 300 Scanning Demonstration: 500
	static int nfeatures = 450; // max amount of features (minimum: 300, otherwise a busy screen will never have enough keypoints to get all areas)
	// Atlantis: 800, Thargos: 800, houston: 600 Scanning Demonstration: 800
	static int nfeaturesInitial = 800; // max amount of features
	static float scalefactor = 1.1f; // scaling factor for each level down in the Size-Pyramid
	static int nlevels = 6; // levels of the size-pyramid
	static int firstLevel = 0; // first level in pyramid (usually 0)
	static float scalefactorInitial = 1.3f; // scaling factor for each level down in the Size-Pyramid
	static int nlevelsInitial = 5; // the reference cards have more levels, beause they are high-res variants
	static int firstLevelInitial = 0; // first level in pyramid (usually 0)
	static int edgeThreshhold = 2; // minimum distance from edge for features (was supposed to be at least patchSize, but lower still seems to produce useful kp on the border!)
	static int patchSize = 32; // size of descriptor patches for each key point. has different reach for each pyramid-level
	static int WTA_K = 2; // this is something too specific to change, just leave it on "2"
	static int fastThreshhold = 20;

	// Score-Types: HARRIS_SCORE gives more stable keypoints while FAST_SCORE has less stable but faster keypoint calculation.
	// since keypoint generation time and keypoint spreading is now less of an issue, Harris score might be the better choice now.
	ORB::ScoreType score_type = ORB::HARRIS_SCORE; //algorithm used to rank keypoints.  only best MAX_FEATURES keypoints are included
	//ORB::ScoreType 
}

namespace
{
	std::default_random_engine createRandomEngine()
	{
		auto seed = static_cast<unsigned long>(std::time(nullptr));
		return std::default_random_engine(seed);
	}

	auto RandomEngine = createRandomEngine();
	static auto rng = std::default_random_engine{};
}

int toPercent(float input) {
	return (int)(100 * input);
}

void ImageComparator::initFromLocalPictures()
{
	cv::Mat loaded_pic = cv::imread("assets/images/0a0se.png", cv::IMREAD_GRAYSCALE);

	/*
	//_android_log_print(ANDROID_LOG_DEBUG, "path", "%s", (path + "/info.txt").c_str());
	ifstream file((path + "/info.txt").c_str());
	file.open();
	if (!file.is_open()) {
		return Object(true);
	}
	*/

	auto simplevec = std::vector<Mat>();
	simplevec.push_back(loaded_pic);
	auto noMasks = std::vector<Mat>();
	initWithPictureList(simplevec, noMasks, true);
	checkUniqueKeypoints(activeCards);
}

void ImageComparator::addNextPictureToScannables(string fileName, bool includingPicture) {
	addNextImageWithName = fileName;
	addNextImageWithNameincludePic = includingPicture;
}
void ImageComparator::addImageWithPrimedName(Mat &pic) {
	addSinglePictureWithoutGroup(pic, addNextImageWithName, addNextImageWithNameincludePic);
	addNextImageWithName = "";
}
// returns index of newly added picture
size_t ImageComparator::addSinglePictureWithoutGroup(cv::Mat &pic, string fileName, bool safePicData)
{
	addName(fileName);
	unsigned int cardIndex = mFilenames.size() - 1;
	// create keypoints using this read data and their alpha masks
	auto simplevec = std::vector<Mat>();
	auto noMasks = std::vector<Mat>();
	simplevec.push_back(pic);
	initWithPictureList(simplevec, noMasks, safePicData);

	// needs to add the group if it doesnt exist! (could do this in the "addCardToGroup" function as well though...)
	addCardToGroup("additionalCustomScans", fileName, cardIndex);

	return cardIndex;
}
bool ImageComparator::removeScannableByName(std::string fileName)
{
	auto foundIdx = std::find(mFilenames.begin(), mFilenames.end(), fileName);
	if (foundIdx != mFilenames.end()) {
		unsigned int cardIndex = foundIdx - mFilenames.begin();
		return removeScannableByIndex(cardIndex);
	}
	return false;
}
bool ImageComparator::removeScannableFromBack()
{
	return removeScannableByIndex(mFilenames.size() - 1);
}
bool ImageComparator::removeScannableByIndex(unsigned int cardIndex)
{
	if (mFilenames.size() <= cardIndex) return false;

	mFilenames.erase(mFilenames.begin() + cardIndex);
	mKeypoints.erase(mKeypoints.begin() + cardIndex);
	mCardSimilarities.erase(mCardSimilarities.begin() + cardIndex);
	mDescriptors.erase(mDescriptors.begin() + cardIndex);
	mRequiredBuckets.erase(mRequiredBuckets.begin() + cardIndex);
	mPicCenters.erase(mPicCenters.begin() + cardIndex);
	// only remove pics when they are used (basically only in C++ project!?)
	if (mPics.size() > cardIndex)
	{
		mPics.erase(mPics.begin() + cardIndex);
	}
	// BEFORE correcting indices on other references, remove the card references from its group. 
	deleteScannableFromAllGroupsByIndex(cardIndex);
	// correct all card-indices that come after this card (lower by one) because we are missing one element in all indexed vectors now!
	for (auto indexGroupEntry : cardIndexGroups)
	{
		auto indexGroup = indexGroupEntry.second;
		for (int i = 0; i < indexGroup.size(); i++)
		{
			if (indexGroup[i] > cardIndex)
			{
				indexGroup[i] = indexGroup[i] - 1;
			}
		}
	}
	return true;
}

void ImageComparator::calculateRequiredBuckets(std::vector<KeyPoint>& keypoints){
	std::vector<Point2f> pnts_train;
	for (size_t i = 0; i < keypoints.size(); i++)
	{
		pnts_train.push_back(keypoints[i].pt);
	}
	int filledBuckets = checkFilledBuckets(currentPictureSize, bucket_size_multiplier_train, pnts_train);
	int minBucketsPossible = ceilf(filledBuckets * bucketMin_train_multiplier_min);
	int requiredBuckets = bucketMin_train_flatmin;
	// if training pic has incredibly few buckets filled, reduced amount of needed buckets
	if (requiredBuckets > minBucketsPossible) requiredBuckets = minBucketsPossible;
	// lets add it
	mRequiredBuckets.push_back(requiredBuckets);
}

bool ImageComparator::initWithPictureList(std::vector<cv::Mat> &pics, std::vector<cv::Mat> &keypointMasks, bool safePicData)
{
	// Detect ORB features and compute descriptors.
	Mat keypointMask = Mat();
	for (size_t i = 0; i < pics.size(); i++)
	{
		cv::Mat& pic = pics[i];
		mKeypoints.push_back(std::vector<cv::KeyPoint>());
		//mKeypointSimilarities.push_back(std::vector<std::vector<SimilarKeypoint>>());
		mCardSimilarities.push_back(std::vector<unsigned int>());
		mDescriptors.push_back(Mat());


		if (keypointMasks.size() > i)
		{
			keypointMask = keypointMasks[i];
		}

		bool prevValue = increaseBrightness;
		increaseBrightness = false;
		preprocessPicture(pic, true, keypointMask);
		increaseBrightness = prevValue;

		currentPictureSize = cv::Point2i(pic.cols, pic.rows);

		detectAndComputeSpreadOutUsingBuckets(mOrbInitial, pic, keypointMask, mKeypoints.back(), mDescriptors.back(), staticNamespace::nfeaturesInitial);
		// find out how many filled buckets are needed for this picture to be recognized
		calculateRequiredBuckets(mKeypoints.back());

		//TagJK_FlannCode
		//mFlannIndex.push_back(cv::makePtr<cv::flann::GenericIndex<cvflann::HammingLUT>>(mDescriptors.back(), *params3));

		mPicCenters.push_back(Point2f((float)pics[i].cols / 2.0f, (float)pics[i].rows / 2.0f));

		// for debugging
		if (safePicData)
		{
			mPics.push_back(pics[i]);
		}
		keypointMask.release();
	}

	// this always needs to happen in the end (even on non-hardcoded cardGroups) to initialize activeCards
	recalculateFilteredCards();
	return true;
}
void ImageComparator::addName(std::string name)
{
	mFilenames.push_back(name);
}
bool ImageComparator::initDebugPicturesOnly(std::vector<cv::Mat> &pics)
{
	// Detect ORB features and compute descriptors.
	for (size_t i = 0; i < pics.size(); i++)
	{
		bool prevValue = increaseBrightness;
		increaseBrightness = false;

		preprocessPicture(pics[i], true);
		increaseBrightness = prevValue;

		// for debugging
		mPics.push_back(pics[i]);
	}
	return true;
}

void ImageComparator::setGammaCorrection(float gamma_correction_level /*lower == brighter, 0.01-1.0*/) {
	gammaCorrectionFactor = gamma_correction_level;

	if (gamma_correction_level != 1.0)
	{
		initGammaLookupTable(gammaCorrectionFactor);
	}
}
void ImageComparator::initGammaLookupTable(float gamma_correction_level /*lower == brighter, 0.01-1.0*/) {
	
	Mat lookUpTable(1, 256, CV_8U);
	uchar* p = lookUpTable.ptr();
	for (int i = 0; i < 256; ++i)
		p[i] = saturate_cast<uchar>(pow(i / 255.0, gamma_correction_level) * 255.0);
	gammaLookupTable = lookUpTable;
}
void ImageComparator::initSearchMask(int width, int height, float radiusPercentOfWidth) {
	if (radiusPercentOfWidth == -1)
	{
		if (!searchMask.empty())
		{
			searchMask = Mat();
		}
		return;
	}
	if (searchMask.cols != height || searchMask.rows != width)
	{
		float xMiddle = 0.5f * width;
		float yMiddle = 0.5f * height;

		float halvedRadius = (radiusPercentOfWidth / 2.0f);
		float allowedDistanceSquared = (halvedRadius * width) * (halvedRadius * width);

		Mat newSearchMask(height, width, CV_8UC1);
		uchar* p = newSearchMask.ptr();
		for (int y = 0; y < height; ++y) {
			for (int x = 0; x < width; ++x) {
				int value = 0;
				float distanceSquared = ((float)x - xMiddle) * ((float)x - xMiddle) + ((float)y - yMiddle) * ((float)y - yMiddle);
				if (allowedDistanceSquared > distanceSquared)
					value = 255;
				p[y * width + x] = (uchar)value;
			}
		}
		searchMask = newSearchMask;
	}
}

ImageComparator::ImageComparator()
{
	searchMask = Mat();
	debugValue = 0;
	debugFlag = false;
	customKpLevels = staticNamespace::nlevels;
	searchMaskSize = -1;// -1 == no searchMask
	scanResolution = STANDARDIZED_RESOLUTION;
	setGammaCorrection(0.5f);
	claheHistogramEqualizer = cv::createCLAHE(2.5f, cv::Size(8, 8));

	increaseBrightness = false; // this was supposed to be set to true for every second picture
	usePreprocessing = true; // default: true
	useSharpening = false; // default: false

	// ** old ORB properties  cv::ORB.create(400, 1.2f, 2, 31, 0, 2, ORB.HARRIS_SCORE, 31, 20)

	// LONG DISTANCE SETTINGS
	//int nfeatures = 500; // max amount of features
	//int nfeaturesInitial = 350; // max amount of features
	//float scalefactor = 1.5f; // scaling factor for each level down in the Size-Pyramid
	//int nlevels = 3; // levels of the size-pyramid
	//int firstLevel = 0; // first level in pyramid (usually 0)
	//float scalefactorInitial = 1.17f; // scaling factor for each level down in the Size-Pyramid
	//int nlevelsInitial = 13; // the reference cards have more levels, beause they are high-res variants
	//int firstLevelInitial = 0; // first level in pyramid (usually 0)
	//int edgeThreshhold = 31; // minimum distance from edge for features (should be at least patchSize)
	//int patchSize = 31; // size of descriptor patches for each key point. has different reach for each pyramid-level


	// SHORT DISTANCE SETTINGS
	//int nfeatures = 350; // max amount of features
	//int nfeaturesInitial = 500; // max amount of features
	//float scalefactor = 1.2f; // scaling factor for each level down in the Size-Pyramid
	//int nlevels = 12; // levels of the size-pyramid
	//int firstLevel = 0; // first level in pyramid (usually 0)
	//float scalefactorInitial = 1.2f; // scaling factor for each level down in the Size-Pyramid
	//int nlevelsInitial = 8; // the reference cards have more levels, beause they are high-res variants
	//int firstLevelInitial = 0; // first level in pyramid (usually 0)
	//int edgeThreshhold = 41; // minimum distance from edge for features (should be at least patchSize)
	//int patchSize = 41; // size of descriptor patches for each key point. has different reach for each pyramid-level
	using staticNamespace::edgeThreshhold;
	using staticNamespace::fastThreshhold;
	using staticNamespace::firstLevel;
	using staticNamespace::firstLevelInitial;
	using staticNamespace::nfeatures;
	using staticNamespace::nfeaturesInitial;
	using staticNamespace::nlevels;
	using staticNamespace::nlevelsInitial;
	using staticNamespace::patchSize;
	using staticNamespace::scalefactor;
	using staticNamespace::scalefactorInitial;
	using staticNamespace::score_type;
	using staticNamespace::spreadOutMultiplier;
	using staticNamespace::WTA_K;

	using namespace algs;
	mBADDescriptor = BAD::create(1.0f, BAD::SIZE_256_BITS);
	mOrbInitial = ORB::create(spreadOutMultiplier * nfeaturesInitial, scalefactorInitial, nlevelsInitial, edgeThreshhold,
		firstLevelInitial, WTA_K, score_type, patchSize, fastThreshhold);
	mOrbCompare = ORB::create(spreadOutMultiplier * nfeatures, scalefactor, nlevels, edgeThreshhold,
		firstLevel, WTA_K, score_type, patchSize, fastThreshhold);
	mMatcher = DescriptorMatcher::create("BruteForce-Hamming(2)"); //("BruteForce-Hamming");"-SL2"
	//mMatcher2 = DescriptorMatcher::create("BruteForce-Hamming"); //("BruteForce-Hamming");"-SL2"

	//params2 = cv::makePtr < cvflann::LshIndexParams>(12, 20, 1);

	// if we wanted to try out Flann matcher again (or any of the other variants via the a GenericIndex)
	// we have to uncomment "//TagJK_FlannCode" line
	//TagJK_FlannCode
	//params3 = cv::makePtr <cvflann::LinearIndexParams>();
	//params = cv::makePtr<cvflann::HierarchicalClusteringIndexParams>(32, cvflann::flann_centers_init_t::FLANN_CENTERS_RANDOM, 4, 100);
	//mFlannIndex = std::vector<Ptr<cv::flann::GenericIndex<cvflann::HammingLUT>>>();
}
void ImageComparator::setCustomKpLevels(int value) {
	customKpLevels = value;
	using staticNamespace::edgeThreshhold;
	using staticNamespace::fastThreshhold;
	using staticNamespace::firstLevel;
	using staticNamespace::nfeatures;
	using staticNamespace::nlevels;
	using staticNamespace::patchSize;
	using staticNamespace::scalefactor;
	using staticNamespace::score_type;
	using staticNamespace::spreadOutMultiplier;
	using staticNamespace::WTA_K;
	int firstLevelOverride = customKpLevels - nlevels;
	if (firstLevelOverride < 0)
		firstLevelOverride = 0;
	mOrbCompare = ORB::create(spreadOutMultiplier * nfeatures, scalefactor, customKpLevels, edgeThreshhold,
		firstLevelOverride, WTA_K, score_type, patchSize, fastThreshhold);
}
void ImageComparator::setScanResolution(float value) {
	scanResolution = value;
}
void ImageComparator::setSearchMaskRadiusPercent(float radiusPercentOfWidth) {
	searchMaskSize = radiusPercentOfWidth;
}

ImageComparator::~ImageComparator()
{
}

float normalizeRotation(float rotationValue) {
	if (rotationValue < 0)
	{
		rotationValue += 360;
	}
	if (rotationValue > 360)
	{
		rotationValue -= 360;
	}
	return rotationValue;
}
bool ImageComparator::checkRotationSimilar(float rot1, float rot2) {
	double rotDifference = abs(rot1 - rot2);
	rotDifference = normalizeRotation(rotDifference);
	return (rotDifference < 10 || rotDifference > 350);
}
bool ImageComparator::checkScaleSimilar(float scale1, float scale2) {
	double scaleDifference = abs(scale2 - scale1);
	return scaleDifference < 0.2;
}

int ImageComparator::checkFilledBuckets(cv::Point2f cardSize, float bucketSizeMultiplier, std::vector<Point2f> keypointPoints, int earlyCancelValue){
	
	std::map<int, int> hashmap_buckets = std::map<int, int>();
	for (auto& pnt : keypointPoints)
	{
		int bucketIndex = findAreaSpreadBucket(cardSize, pnt, bucketSizeMultiplier);
		hashmap_buckets[bucketIndex] = 1;
		if (hashmap_buckets.size() >= earlyCancelValue)
		{
			break;
		}
	}
	return hashmap_buckets.size();
}


struct MatchKeypointResolutionComparator
{
	MatchKeypointResolutionComparator(const std::vector<KeyPoint>& i_keypoints1, const std::vector<KeyPoint>& i_keypoints2) : keypoints1(i_keypoints1), keypoints2(i_keypoints2){};
	bool operator()(DMatch& a, DMatch& b) {
		float multipliedResponse_a = (keypoints1[a.queryIdx].response * keypoints2[a.trainIdx].response);
		float multipliedResponse_b = (keypoints1[b.queryIdx].response * keypoints2[b.trainIdx].response);
		return multipliedResponse_a > multipliedResponse_b;
	}

	const std::vector<KeyPoint>& keypoints1;
	const std::vector<KeyPoint>& keypoints2;
};
void ImageComparator::matchResults(
	const std::vector<KeyPoint>& keypoints1 /*query pic, the one newly scanned*/,
	const Mat& descriptors1,
	const std::vector<KeyPoint>& keypoints2/*train-pic, the one trained beforehand*/,
	const Mat& descriptors2,
	int minimumMatches,
	Point2f middle_of_reference_card,
	std::vector<ComparisonData>& dataVec,
	int idxComparePic, /*index of the trained pic*/
	double resolutionDiagonal, /* diagonale length of camera resolution. relevant for everything distance based*/
	bool useALLMatches /*= false, if set to true will ignore the one match per keypoint rule*/)
{
	if (descriptors2.empty()) {
		return;
	}

	// Match features.
	std::vector<DMatch> original_matches;

	float KNN_MatchPercent = GOOD_MATCH_PERCENT_KNN;
	// matching
	bool useKnnMatcher = true;
	if (useKnnMatcher)
	{
		std::vector< std::vector<DMatch> > knn_matches;

		/*
		//TagJK_FlannCode
		if (false) {
			int knn = 2;
			Mat indices(descriptors1.rows, knn, CV_32S);// indices into features array - integers
			Mat distances(descriptors1.rows, knn, CV_32S);// distances - floats (even with integer data distances are floats)
			mFlannIndex[idxComparePic]->knnSearch(descriptors1, indices, distances, knn, ::cvflann::SearchParams());
			const float ratio_thresh = 0.89f;
			for (int i = 0; i < indices.rows; ++i)
			{
				//https://github.com/introlab/find-object/blob/master/example/main.cpp

				if (indices.at<int>(i, 0) >= 0 && indices.at<int>(i, 1) >= 0 &&
					distances.at<int>(i, 0) <= ratio_thresh * distances.at<int>(i, 1))
				{
					int imgIdx = 0;
					original_matches.push_back(DMatch(i, indices.at<int>(i, 0), imgIdx, distances.at<int>(i, 0)));
				}
			}
			indices.release();
			distances.release();
		}*/


		mMatcher->knnMatch(descriptors1, descriptors2, knn_matches, 2);
		//-- Filter matches using the Lowe's ratio test
		const float ratio_thresh = 0.89f;
		for (size_t i = 0; i < knn_matches.size(); i++)
		{
			if (knn_matches[i].size() >= 2)
			{
				if ((float)knn_matches[i][0].distance < ratio_thresh * (float)knn_matches[i][1].distance)
				{
					original_matches.push_back(knn_matches[i][0]);
				}
			}
		}
		mMatcher->clear();
	}
	else {
		mMatcher->match(descriptors1, descriptors2, original_matches, Mat());
		mMatcher->clear();
	}

	// Sort matches by score
	std::sort(original_matches.begin(), original_matches.end());

	// Remove not so good matches
	int numGoodMatches = 0;
	if (useKnnMatcher)
	{
		numGoodMatches = (int)(original_matches.size() * KNN_MatchPercent);
	} else {
		numGoodMatches = (int)(original_matches.size() * GOOD_MATCH_PERCENT);
	}
	original_matches.erase(original_matches.begin() + numGoodMatches, original_matches.end());

	// if multiple matches match the same training-keypoint (reference picture), only keep the best
	std::vector<DMatch> matches;
	if (!useALLMatches)
	{
		for (auto& match : original_matches)
		{
			auto* accepted_match = &match;
			float lowest_accepted_distance = match.distance;
			for (auto& match2 : original_matches)
			{	// check if any match (except for this one) has the same matched training-keypoint
				if (match.queryIdx != match2.queryIdx && match.trainIdx == match2.trainIdx && match2.distance < lowest_accepted_distance)
				{
					accepted_match = &match2;
					lowest_accepted_distance = match2.distance;
				}
			}
			// add only if not yet added
			bool already_added = false;
			for (auto& inserted_match : matches) {
				if (inserted_match.queryIdx == accepted_match->queryIdx && inserted_match.trainIdx == accepted_match->trainIdx)
				{
					already_added = true;
					break;
				}
			}
			if (!already_added)
			{
				matches.push_back(*accepted_match);
			}
		}
	}
	else {
		matches = original_matches;
	}
	
	// no matches? no point in going any further then..
	if ((int)matches.size() < (int)minimumMatches) {
		return;
	}

	std::vector<Point2f> origins2(matches.size());
	std::vector<Point2f> matchOrientationVecs(matches.size()); // contains a normalized vector for each match, pointing into the direction of orientation
	std::vector<float> matchRotations(matches.size());
	std::vector<float> matchScales(matches.size());
	for (size_t i = 0; i < matches.size(); i++)
	{
		auto& curr_match = matches[i];
		int i_train = curr_match.trainIdx;
		int i_query = curr_match.queryIdx;
		//rotate original keypoint vec by primary rotation
		float scale = keypoints1[i_query].size / keypoints2[i_train].size;
		matchScales[i] = scale;
		//TagJK_continue use average scale instead of per-point scale here? not sure about this one..
		float rotation_angle = keypoints1[i_query].angle - keypoints2[i_train].angle;
		matchRotations[i] = rotation_angle;
		double cs = cos(rotation_angle * PI / 180.0f);
		double sn = sin(rotation_angle * PI / 180.0f);
		auto original_vec = middle_of_reference_card - keypoints2[i_train].pt;
		float px = (float)(original_vec.x * cs - original_vec.y * sn);
		float py = (float)(original_vec.x * sn + original_vec.y * cs);
		auto rotated_vec = Point2f(px, py);
		matchOrientationVecs[i] = Point2f((float)cs, (float)sn);
		auto final_origin = keypoints1[i_query].pt + rotated_vec * scale;
		origins2[i] = final_origin;
		//keypoints1[i_query].pt = final_origin;
	}

	// k-means example (not as useful though because it doesnt actually find cluster CENTERS)
	//cv::Mat2f kMeansInput = cv::Mat2f(origins2, false);
	//int clusterCount = 1;
	//std::vector<Point2f> centers;
	//Mat labels;
	//double compactness = kmeans(origins2, clusterCount, labels/*labels - can check this mat at positions, to see wich point belongs to wich cluster*/,
	//	TermCriteria(TermCriteria::EPS + TermCriteria::COUNT, 10, 1.0),
	//	3, KMEANS_PP_CENTERS, centers);

	// use meanshift clustering instead
	double searchRadius = (clustering_diameter * resolutionDiagonal);
	double searchRadiusSquared = searchRadius * searchRadius;
	double epsilon = resolutionDiagonal / 800.0f * 1.0f; // original: 1.0f originally was 0.01f but very low numbers can increase the runtime immensly for minor improvement of positioning
	std::vector<Point2f> centers = MeanShift::meanshift(origins2, 1.5 * searchRadius, epsilon, minimumMatches);

	size_t originSize = origins2.size();
	std::vector<Point2f> originsFinal;
	std::vector<Point2f> rotationsFinal;
	std::vector<float> scalesFinal;
	std::vector<int> closeIndices;
	for (auto center : centers)
	{
		// find average rotations and scales
		std::vector<DMatch> finalMatches;
		Point2f avgRotation = Point2f(0, 0); // vector roughly pointing into the average orientation
		double avgScale = 0;
		int closePoints = 0;
		for (size_t i = 0; i < originSize; i++)
		{
			float distance = (origins2[i].x - center.x) * (origins2[i].x - center.x) +
 				(origins2[i].y - center.y) * (origins2[i].y - center.y);

			//TagJK_rethink should this include a multiplication by matchScales[i] so the search radius is relative to the card instead of the picture?
			if (distance < searchRadiusSquared)
			{
				//auto& curr_match = matches[i];
				avgRotation += matchOrientationVecs[i];
				avgScale += matchScales[i];
				closePoints++;
				//int i_query = curr_match.queryIdx;
				//keypoints1[i_query].size = 0;
				closeIndices.push_back(i);
			}
		}
		// sort out keypoints that dont match the average rotation and average scale
		double angleInRadians = std::atan2(avgRotation.y, avgRotation.x);
		double avgRotAngle = (angleInRadians / PI) * 180.0;
		avgScale /= (double)closePoints;

		for (int i : closeIndices)
		{
			bool rotationSimilar = checkRotationSimilar(avgRotAngle, matchRotations[i]);
			bool scaleSimilar = checkScaleSimilar(avgScale, matchScales[i]);
			if (rotationSimilar && scaleSimilar)
			{
				finalMatches.push_back(matches[i]);
				originsFinal.push_back(origins2[i]);
				rotationsFinal.push_back(matchOrientationVecs[i]);
				scalesFinal.push_back(matchScales[i]);
			}
		}
		
		if (finalMatches.size() > 20)
		{
			int i = 0;
		}
		// sort matches by original keypoint-resolution
		//std::sort(finalMatches.begin(), finalMatches.end(), MatchKeypointResolutionComparator(keypoints1, keypoints2));

		// final calculation of orientation and scale. using only the (response-wise) best values
		
		Point2f avgFinalPosition = Point2f(0,0);
		Point2f rotationVec = Point2f(0, 0);
		float avgFinalScale = 0;
		float responseStrengthSum = 0;
		for (size_t i = 0; i < finalMatches.size(); i++)
		{
			auto& curr_match = finalMatches[i];
			int i_train = curr_match.trainIdx;
			int i_query = curr_match.queryIdx;

			//float responseStrength = (keypoints1[i_query].response + keypoints2[i_train].response);
			rotationVec += rotationsFinal[i];
			avgFinalScale += scalesFinal[i];
			avgFinalPosition += originsFinal[i];
		}
		rotationVec /= (float)finalMatches.size();
		double avgFinalRotation = (std::atan2(rotationVec.y, rotationVec.x) / PI) * 180.0;
		avgFinalScale /= (float)finalMatches.size();
		avgFinalPosition.x /= (float)finalMatches.size();
		avgFinalPosition.y /= (float)finalMatches.size();
		
		// calculate spread of keypoints across the image
		// too small a diameter often means we scanned a buttload of keypoints inside someones hair or other busy parts of the image
		std::vector<Point2f> pnts_query;
		std::vector<Point2f> pnts_train;
		for (size_t i = 0; i < finalMatches.size(); i++)
		{
			int i_query = finalMatches[i].queryIdx;
			pnts_query.push_back(keypoints1[i_query].pt);
			int i_train = finalMatches[i].trainIdx;
			pnts_train.push_back(keypoints2[i_train].pt);
		}

		// check query picture filled buckets (how many spots on the image have recognized keypoints within)
		int minimumNeededQueryBuckets = minimumMatches < bucketMin_query? minimumMatches : bucketMin_query;
		int filledBuckets_query = checkFilledBuckets(currentPictureSize, bucket_size_multiplier_query, pnts_query, minimumNeededQueryBuckets);// can put early cancel value to safe some CPU
		bool queryKeypointsAreSpreadWell = filledBuckets_query >= minimumNeededQueryBuckets;

		// check training picture filled buckets (how many spots on the image have recognized keypoints within)
		cv::Point2f cardSize_train = 2.0f * mPicCenters[idxComparePic];
		int minimumNeededTrainBuckets = minimumMatches < mRequiredBuckets[idxComparePic] ? minimumMatches : mRequiredBuckets[idxComparePic];
		int filledBuckets_train = checkFilledBuckets(cardSize_train, bucket_size_multiplier_train, pnts_train, minimumNeededTrainBuckets);// can put early cancel value to safe some CPU
		bool trainKeypointsAreSpreadWell = filledBuckets_train >= minimumNeededTrainBuckets;

		// reduce impact of matches that are commonly found in other pictures too
		float finalRating = 0;/*
		if (!useALLMatches)
		{
			auto& keypointSimilaritySet = mKeypointSimilarities[idxComparePic];
			std::vector<unsigned int> similarPicturesFound = std::vector<unsigned int>();
			for (auto match : finalMatches)
			{
				finalRating += 1.0f;
				bool multipleSimilarPicturesUsingThisKeypoint = false;
				auto& specificKeypointSimilarities = keypointSimilaritySet[match.trainIdx];
				for (auto similarity : specificKeypointSimilarities) {
					if (std::find(similarPicturesFound.begin(), similarPicturesFound.end(), similarity.idSimilarKeypointSet) == similarPicturesFound.end()) {
						similarPicturesFound.push_back(similarity.idSimilarKeypointSet);
					}
					else {
						// ignore multiple keypoints with similarity to the same picture!
						multipleSimilarPicturesUsingThisKeypoint = true;
					}
				}
				if (multipleSimilarPicturesUsingThisKeypoint)
				{
					finalRating -= 1;
				}
			}
		}
		else {*/
			finalRating = (float)finalMatches.size();/*
		}
		if (finalRating / (float)finalMatches.size() < 0.5f)
		{
			// dont allow results where less than x% is unique keypoints
			finalRating = 0;
		}*/

		if (finalRating >= minimumMatches && queryKeypointsAreSpreadWell && trainKeypointsAreSpreadWell)
		{
			dataVec.push_back(ComparisonData());
			ComparisonData& currentData = dataVec.back();

			currentData.id_original_card = idxComparePic;
			currentData.matches = finalMatches;
			currentData.matches_final = (int)currentData.matches.size();
			currentData.match_rating = finalRating;
			currentData.matches_found = (int)matches.size();
			currentData.similarity_original_card = toPercent(currentData.match_rating); //TagJK_deprecated?
			currentData.position_original_card = avgFinalPosition;
			currentData.rotation_original_card = avgFinalRotation;
			currentData.scale_original_card = avgFinalScale;
		}
	}

}

int ImageComparator::findAreaSpreadBucket(cv::Point2f cardSize, cv::Point2f point, float bucketSizeMultiplier) {
	float shorterSide = cardSize.x;
	if (cardSize.y < shorterSide) shorterSide = cardSize.y;
	float bucketSize = shorterSide * bucketSizeMultiplier;
	int xAxisBuckets = (int)ceilf(1.0f / bucketSizeMultiplier);
	int bucketIndex = (int)floorf(point.y / bucketSize) * xAxisBuckets + (int)floorf(point.x / bucketSize);
	return bucketIndex;
}
void ImageComparator::recalculateOriginGroups() {

	std::vector<std::string> allOriginGroups = std::vector<std::string>();
	for (const auto& cardGroupMapEntry : cardIndexGroups)
	{
		const std::string groupName = cardGroupMapEntry.first;

		bool isChild = false;
		for (const auto& childrenGroupsMapEntry : childrenGroups)
		{
			const std::vector <std::string>& childrenGroups = childrenGroupsMapEntry.second;
			if (std::find(childrenGroups.begin(), childrenGroups.end(), groupName) != childrenGroups.end())
			{
				isChild = true;
			}
		}
		if (!isChild)
		{
			allOriginGroups.push_back(groupName);
		}
	}
	originGroups = allOriginGroups;
}
void ImageComparator::addCardToGroup(string groupName, string cardName, unsigned int cardIndex) {
	addCardGroup(groupName);
	cardIndexGroups[groupName].push_back(cardIndex);
}
void ImageComparator::deleteScannableFromAllGroupsByIndex(unsigned int cardIndex) {

	for (auto cardIndexGroupEntry : cardIndexGroups)
	{
		auto cardIndexGroup = cardIndexGroupEntry.second;
		auto foundIdx = std::find(cardIndexGroup.begin(), cardIndexGroup.end(), cardIndex);
		if (foundIdx != cardIndexGroup.end())
		{
			cardIndexGroup.erase(foundIdx);
		}
	}
}
// adds new group and returns current cardGroupIndex
void ImageComparator::addCardGroup(std::string groupName) {
	if (cardIndexGroups.count(groupName)) return; // already exists
	addCardGroup(groupName, std::vector<unsigned int>(), std::vector<std::string>(), false);
}
// adds new group and returns current cardGroupIndex
size_t ImageComparator::addCardGroup(std::string groupName, std::vector<unsigned int> cardIndices, std::vector<std::string> childGroups, bool allowCardGroupEarlyCancel) {
	cardGroupEarlyCancelAllowance[groupName] = allowCardGroupEarlyCancel; // allow early cancel because cards are dissimilar enough to depend on results
	cardIndexGroups[groupName] = cardIndices;
	childrenGroups[groupName] = childGroups;
	recentResults[groupName] = RecentScan();
	groupNames.push_back(groupName);
	recalculateOriginGroups();
	return cardIndices.size() - 1; // return cardGroupIndex
}
void ImageComparator::clearAllCardGroups() {
	cardGroupEarlyCancelAllowance.clear();
	groupNames.clear();
	cardIndexGroups.clear();
	recentResults.clear();
	childrenGroups.clear();
	setCardFilter(FILTER_MODE::ResetFilters);
	recalculateFilteredCards();
}
bool ImageComparator::setCardFilter(FILTER_MODE::Enum filter, std::string groupName)
{
	if (groupName != "" && !cardIndexGroups.count(groupName))
		return false;

	switch (filter)
	{
	case FILTER_MODE::AddAllowance:
		allowedCardGroupsFilter.push_back(groupName);
		break;
	case FILTER_MODE::AddDisallowance:
		disallowedCardGroupsFilter.push_back(groupName);
		break;
	case FILTER_MODE::ResetFilters:
		disallowedCardGroupsFilter.clear();
		allowedCardGroupsFilter.clear();
		break;
	default:
		break;
	}

	recalculateFilteredCards();
	return true;
}

void ImageComparator::recalculateFilteredCards()
{
	activeCards.clear();
	earlyCancelOnActiveCards = false;
	unsigned int cardIdx = 0;
	bool atLeastOneGroupDissallowsEarlyCancel = false;
	for (const auto &filename : mFilenames)
	{
		bool cardAllowed = true;
		for (auto disallowedCardgroup : disallowedCardGroupsFilter)
		{
			for (auto cardIndex : cardIndexGroups[disallowedCardgroup])
			{
				if (mFilenames[cardIndex] == filename)
				{
					cardAllowed = false;
				};
			};
		}
		if (!allowedCardGroupsFilter.empty())
		{
			bool cardInAllowanceGroups = false;
			for (auto allowedCardgroup : allowedCardGroupsFilter)
			{
				for (auto cardIndex : cardIndexGroups[allowedCardgroup])
				{
					if (mFilenames[cardIndex] == filename)
					{
						cardInAllowanceGroups = true;
						// check if this card group disallows early cancel
						if (cardGroupEarlyCancelAllowance[allowedCardgroup] == false)
						{
							atLeastOneGroupDissallowsEarlyCancel = true;
						}
					};
				};
			}
			if (!cardInAllowanceGroups)
			{
				cardAllowed = false;
			}
		}

		if (cardAllowed)
		{
			activeCards.push_back(cardIdx);
		}

		cardIdx++;
	}
	// if no group disallows early cancel, we are good to go on that!
	earlyCancelOnActiveCards = !atLeastOneGroupDissallowsEarlyCancel;
}

// before calling this, set ALL cards active, that can feasibly interact with another at any time
// if two completely distinct sets of cards exist, activate one set first, call checkUniqueneKeypoints() and then set the other set and call checkUniqueneKeypoints() again.
const void ImageComparator::checkUniqueKeypoints(std::vector<unsigned int> &indicesToCheckAgainstEachother) {
	int minimumMatches = 3; // less than usual because we want to find bad results intentionally

	for (size_t idxActualPic : indicesToCheckAgainstEachother)
	{
		auto& actualKeypoints = mKeypoints[idxActualPic];
		auto& actualDescriptors = mDescriptors[idxActualPic];

		float resolutionDiagonal = 2.0f * sqrt(mPicCenters[idxActualPic].x * mPicCenters[idxActualPic].x + mPicCenters[idxActualPic].y * mPicCenters[idxActualPic].y);
		currentPictureSize = cv::Point2i(2.0f * mPicCenters[idxActualPic].x, 2.0f * mPicCenters[idxActualPic].y);

		std::vector<ComparisonData> dataVec;

		for (size_t idxComparePic : indicesToCheckAgainstEachother)
		{
			// dont check against yourself
			if (idxComparePic == idxActualPic) continue;
			
			auto& refKeypoints = mKeypoints[idxComparePic];
			auto& refDescriptors = mDescriptors[idxComparePic];

			matchResults(actualKeypoints, actualDescriptors, refKeypoints, refDescriptors, minimumMatches, mPicCenters[idxComparePic], dataVec, (int)idxComparePic, resolutionDiagonal, true);
		}

		//auto& keypointSet = mKeypointSimilarities[idxActualPic];
		auto& cardSimilarities = mCardSimilarities[idxActualPic];
		for (auto& data : dataVec)
		{	
			// remember specific keypoint connections
			/*
			for (auto& match : data.matches)
			{
				SimilarKeypoint simKeypoint = SimilarKeypoint(data.id_original_card, match.trainIdx);
				auto& specificKeypointSimilarities = keypointSet[match.queryIdx];
				// add only unique
				if (std::find(specificKeypointSimilarities.begin(), specificKeypointSimilarities.end(), simKeypoint) == specificKeypointSimilarities.end()) {
					specificKeypointSimilarities.push_back(simKeypoint);
				}

				auto& keypointSet2 = mKeypointSimilarities[data.id_original_card];
				SimilarKeypoint simKeypoint2 = SimilarKeypoint(idxActualPic, match.queryIdx);
				auto& specificKeypointSimilarities2 = keypointSet2[match.trainIdx];
				if (std::find(specificKeypointSimilarities2.begin(), specificKeypointSimilarities2.end(), simKeypoint2) == specificKeypointSimilarities2.end()) {
					specificKeypointSimilarities2.push_back(simKeypoint2);
				}
			}*/
			// remember similar card connections
			if (std::find(cardSimilarities.begin(), cardSimilarities.end(), data.id_original_card) == cardSimilarities.end()) {
				cardSimilarities.push_back(data.id_original_card);
			}
			auto& cardSimilarities2 = mCardSimilarities[data.id_original_card];
			if (std::find(cardSimilarities2.begin(), cardSimilarities2.end(), idxActualPic) == cardSimilarities2.end()) {
				cardSimilarities2.push_back((int)idxActualPic);
			}
		}
	}
}

// CAREFUL this needs gray images!
void ImageComparator::preprocessPicture(cv::Mat& pic, bool initializingCard, cv::Mat& mask) {
	//increaseBrightness = !increaseBrightness;
	// ~0.1ms
	int biggerSize = pic.cols;
	if (pic.rows > pic.cols) biggerSize = pic.rows;

	double standardizedResolution = scanResolution;
	if (initializingCard)
	{
		standardizedResolution = KEYPOINT_GENEREATION_RESOLUTION;
	}
	double resolutionMultiplier = standardizedResolution / biggerSize;
	if (resolutionMultiplier != 1.0)
	{
		if (!pic.empty())
		{
			cv::resize(pic, pic, cv::Size(), resolutionMultiplier, resolutionMultiplier);
		}
		if (!mask.empty())
		{
			cv::resize(mask, mask, cv::Size(), resolutionMultiplier, resolutionMultiplier);
		}
	}

	if (increaseBrightness)
	{
		if (gammaCorrectionFactor != 1.0)
		{
			LUT(pic, gammaLookupTable, pic); // brightness correction
		}
	}
	// ~0.6ms
	if (usePreprocessing)
	{
		//Mat lab;
		//cvtColor(pic, lab, COLOR_RGB2Lab);
		//Mat ch1;
		//int channelIdx = 0;
		//extractChannel(lab, ch1, channelIdx); // extract specific channel
		//claheHistogramEqualizer->apply(ch1, ch1); // increase contrast
		//
		//cvtColor(lab, pic, COLOR_Lab2BGR);

		//claheHistogramEqualizer->apply(pic, pic); // increase contrast
		if (initializingCard)
		{
			Ptr<cv::CLAHE> cardInitClaheHistogramEqualizer = cv::createCLAHE(1.5f, cv::Size(8, 8));
			cardInitClaheHistogramEqualizer->apply(pic, pic); // increase contrast
		}
		else {
			claheHistogramEqualizer->apply(pic, pic); // increase contrast
		}
		//equalizeHist(pic, pic); // CAREFUL: this requires GRAY-scale pics
	}
	// ~0.8ms
	if (useSharpening)
	{
		//Mat sharp;
		//Mat sharpening_kernel = (Mat_<double>(3, 3) << -0.3, -0.3, -0.3, -0.3, 3.4, -0.3, -0.3, -0.3, -0.3);
		//filter2D(pic, pic, -1, sharpening_kernel);
		double sigma = 2.5, amount = 1;
		Mat blurry;
		GaussianBlur(pic, blurry, cv::Size(), sigma);
		addWeighted(pic, 1 + amount, blurry, -amount, 0, pic);
		blurry.release();

		//cv::GaussianBlur(frame, image, cv::Size(0, 0), 11);
		//cv::addWeighted(frame, 1.5, image, -0.5, 0, image);
	}
}
void ImageComparator::preprocessPictureRemoveStaticBackground(cv::Mat &pic) {
	if (lastPic.rows != pic.rows
		|| lastPic.cols != pic.cols)
	{
		pic.copyTo(lastPic);
	}
	cv::Mat temp_pic;
	absdiff(pic, lastPic, temp_pic);
	pic.copyTo(lastPic);
	cv::Mat changed = temp_pic < 20; // find all similar pixels ("10" just being an amount of tolerance to change)
	changed *= (1.0 / 255.0); // change results from 255 to 1
	if (standstillData.rows != pic.rows
		|| standstillData.cols != pic.cols)
	{
		standstillData = cv::Mat::zeros(cv::Size(lastPic.cols, lastPic.rows), changed.type());
	}
	standstillData += changed; // increment unchanged-pixel info
	standstillData = standstillData.mul(changed); // reset all other pixel info
	// reuse changed-mat to get static background places
	changed = standstillData < 50; // find all places that havent moved in the past 5 frames
	changed *= (1.0 / 255.0); // change results from 255 to 1
	
	multiply(pic, changed.getUMat(ACCESS_READ), pic);
}

bool ImageComparator::wasRecentlyScanned(RecentScan recentScan) {
	return recentScan.cardIdx != 65535 && (recentScan.lastValidScanIteration + 1 == scanIteration || recentScan.lastValidScanIteration + 2 == scanIteration);
}

void ImageComparator::iterativeGroupBasedDetection(
	std::vector<std::string>& iGroupNames, 
	std::vector<KeyPoint>& query_keypoints /*query pic, the one newly scanned*/,
	Mat& descriptors,
	double resolutionDiagonal,
	std::vector<ComparisonData>& dataVecOut,
	bool innerGroupTest, /* = false*/
	bool singlePictureOnly /* = false*/) 
{
	if (iGroupNames.size() == 0) return;

	// copy recieved group list and shuffle it
	std::vector<std::string> targetGroupNames = iGroupNames; // copy list before shuffling it
	std::shuffle(std::begin(targetGroupNames), std::end(targetGroupNames), rng);

	// put the group with the most recent successful scan to the front
	{ 
		int latestSuccessfulScanIteration = 0;
		int latestSuccessfulGroup = 0;
		for (int i = 0; i < targetGroupNames.size(); i++)
		{
			RecentScan& lastScannedInThisGroup = recentResults[targetGroupNames[i]];
			if (wasRecentlyScanned(lastScannedInThisGroup) && lastScannedInThisGroup.lastValidScanIteration > latestSuccessfulScanIteration) {
				latestSuccessfulScanIteration = lastScannedInThisGroup.lastValidScanIteration;
				latestSuccessfulGroup = i;
			}
		}
		std::string emplaceTemp = targetGroupNames[0];
		targetGroupNames[0] = targetGroupNames[latestSuccessfulGroup];
		targetGroupNames[latestSuccessfulGroup] = emplaceTemp;
		latestSuccessfulGroup = 0;
	}

	// check all groups front to back for successful scans within
	for (const auto & groupName : targetGroupNames)
	{
		{ // check if group is active
			if (allowedCardGroupsFilter.size() > 0)
			{
				auto currentVecPos = std::find(allowedCardGroupsFilter.begin(), allowedCardGroupsFilter.end(), groupName);
				// if not explicitly allowed but at least one allowfilter is on, ignore this group
				if (currentVecPos == allowedCardGroupsFilter.end()) continue;
			}
			auto currentVecPos = std::find(disallowedCardGroupsFilter.begin(), disallowedCardGroupsFilter.end(), groupName);
			// if disallowed, ignore this group
			if (currentVecPos != disallowedCardGroupsFilter.end()) continue;
		}

		bool allowEarlyCancel = cardGroupEarlyCancelAllowance[groupName];
		std::vector<unsigned int> activeCardSet = cardIndexGroups[groupName];


		RecentScan &lastScannedInThisGroup = recentResults[groupName];
		bool lastScannedInThisGroupVALID = false;
		if (wasRecentlyScanned(lastScannedInThisGroup)) {
			// we found a valid lastScanned (just keep it in there)
			lastScannedInThisGroupVALID = true;
		}
		// for debugging respective concepts, uncomment the following lines
		//lastScannedInThisGroupVALID = false;
		//innerGroupTest = false;
			

		std::vector<ComparisonData> dataVec = std::vector<ComparisonData>();

		// if singlePictureOnly is active, we will stop on the first successful match, check against any known similar 
		// keypointSets and then return without checking any further. randomization avoids constantly long search times for items in the back of the list.
		if (singlePictureOnly) {
			std::shuffle(std::begin(activeCardSet), std::end(activeCardSet), rng);

			// after randomization, push the current partial result target to the front for faster scan-return
			if (lastScannedInThisGroupVALID)
			{
				lastScannedInThisGroupVALID = false; // if this cardIdx is not found, we return to false
				auto currentVecPos = std::find(activeCardSet.begin(), activeCardSet.end(), lastScannedInThisGroup.cardIdx);
				if (currentVecPos != activeCardSet.end())
				{
					unsigned int firstValue = activeCardSet[0];
					*currentVecPos = firstValue;
					activeCardSet[0] = lastScannedInThisGroup.cardIdx;
					lastScannedInThisGroupVALID = true; // if this cardIdx is found, go back to true again and continue
				}
			}
		}

		//matchResults(query_keypoints, descriptors, refKeypoints, refDescriptors, minimum_diameter, minimumMatches, mPicCenters[idxComparePic], dataVec, idxComparePic);
		int actualRequiredMatchCount = minimum_match_count;
		int firstCheckFastCancel = -1;
		float multiscanGroupMatchMultiplier = 0.5;
		float lastScannedMatchMultiplier = 0.35;
		if (lastScannedInThisGroupVALID)
		{
			firstCheckFastCancel = lastScannedMatchMultiplier * minimum_match_count; // firstCheckFastCancel is allowed on the very first check of findClosestMatchFast
		}
		if (innerGroupTest || (childrenGroups[groupName].size() > 0)) {
			actualRequiredMatchCount = multiscanGroupMatchMultiplier * minimum_match_count; // need less keypoints because we are at least sure this is a card now. So less chance to find something in your hair
		}
		int reducedMinimumMatchCount = actualRequiredMatchCount;
		if (singlePictureOnly)
		{
			// we need a lower threshhold to find and check the "second best" result as well and compare it with the best result
			reducedMinimumMatchCount *= 0.6f;
			if (reducedMinimumMatchCount < 3) reducedMinimumMatchCount = 3;
		}
		std::vector<unsigned int> alreadyChecked = std::vector<unsigned int>();
		findClosestMatchFast(alreadyChecked, activeCardSet, singlePictureOnly, query_keypoints, descriptors, resolutionDiagonal, reducedMinimumMatchCount, firstCheckFastCancel, dataVec);
		// in single search just stop on the first valid target

		std::sort(std::begin(dataVec),
			std::end(dataVec),
			[](ComparisonData& a, ComparisonData& b) {return a.match_rating > b.match_rating; });

		if (singlePictureOnly && dataVec.size() > 1)
		{
			// if first result isnt FAR better than second result, we dont have any legit result
			// FAR better means: either double or x higher, wichever is HIGHER
			float requiredMinimumRating = dataVec[1].match_rating + 15;
			if (dataVec[1].match_rating * 2.0f > requiredMinimumRating)
			{
				requiredMinimumRating = dataVec[1].match_rating * 2.0f;
			}
			// from now on only the first result is relevant, remove all but the first
			dataVec.erase(dataVec.begin() + dataVec.size(), dataVec.end());
			// if the first doesnt match the required distance from the second, remove that as well
			if (dataVec[0].match_rating < requiredMinimumRating)
			{
				dataVec.clear();
			}
		}
		if (singlePictureOnly && dataVec.size() > 0)
		{
			dataVec.resize(1);
			//remove all results below actual minimum threshold
			auto& currentCandidate = dataVec[0];
			if (lastScannedInThisGroupVALID && currentCandidate.id_original_card == lastScannedInThisGroup.cardIdx)
			{
				actualRequiredMatchCount = minimum_match_count * lastScannedMatchMultiplier; // need less keypoints because we are at least sure this is a card now. So less chance to find something in your hair
			}
			if (currentCandidate.match_rating < actualRequiredMatchCount)
			{
				dataVec.clear();
			}
		}
		// if we still have a result by now, its actually a proper result!
		if (singlePictureOnly && dataVec.size() > 0) {
			lastScannedInThisGroup.cardIdx = dataVec[0].id_original_card;
			lastScannedInThisGroup.lastValidScanIteration = scanIteration;
		}


		// check if any any of the results need deeper iteration
		bool resultValid = true;
		for (int i = dataVec.size() - 1; i >= 0; i--)
		{
			std::vector<ComparisonData> innerResults = std::vector<ComparisonData>();
			if (childrenGroups[groupName].size() > 0) {
				iterativeGroupBasedDetection(childrenGroups[groupName], query_keypoints, descriptors, resolutionDiagonal, innerResults, true, singlePictureOnly);
				if (innerResults.size() > 0)
				{
					// can still compare position as well in the future... but this needs thinking about scale implications
					//dataVec[i].position_original_card;
					//innerResults[0].position_original_card;
					bool rotationSimilar = checkRotationSimilar(innerResults[0].rotation_original_card, dataVec[i].rotation_original_card);
					bool scaleSimilar = checkScaleSimilar(innerResults[0].scale_original_card, dataVec[i].scale_original_card);
					// check rotation and scale (unless these checks are flagged to not be relevant in this case via the _folderSettings.txt)
					if ((rotationSimilar || !rotationSameAsFirst) && (scaleSimilar || !scaleSameAsFirst))
					{
						dataVec[i].subResults = innerResults;
					}
					else
					{
						// found child doesnt fit the expected orientation and scale
						dataVec.pop_back();
					}
				} else {
					// expected child but didnt find result in child groups! this invalidates the result
					dataVec.pop_back();
				}
			}
		}
		for (const auto &singleResult : dataVec)
		{
			dataVecOut.push_back(singleResult);
		}
		// if we had at least one successful result in this group, no need to check the other groups!
		if (singlePictureOnly && dataVec.size() > 0)
		{
			return;
		}
	}
}

struct IndexResponseTupel {
	unsigned int index;
	float keypointResponse;
	IndexResponseTupel& operator=(IndexResponseTupel otherStruct) {
		keypointResponse = otherStruct.keypointResponse;
		index = otherStruct.index;
		return *this;
	}
};
bool compareIndexResponseTupel(const IndexResponseTupel& a, const IndexResponseTupel& b) {
	return a.keypointResponse > b.keypointResponse;
};

vector<cv::KeyPoint> sortKeypoints(vector<cv::KeyPoint>& inputKeypoints) {
	int inputSize = inputKeypoints.size();
	if (inputSize <= 1) return inputKeypoints;

	// sort keypoints by response value
	vector<float> responseVector;
	responseVector.reserve(inputSize);
	for (unsigned int i = 0; i < inputSize; i++) {
		responseVector.push_back(inputKeypoints[i].response);
	}
	vector<int> Indx(inputSize);
	for (int i = 0; i < inputSize; i++) {
		Indx[i] = i;
	}
	cv::sortIdx(responseVector, Indx, cv::SORT_DESCENDING);

	vector<cv::KeyPoint> keyPointsSorted;
	keyPointsSorted.reserve(inputSize);
	for (unsigned int i = 0; i < inputSize; i++) {
		keyPointsSorted.push_back(inputKeypoints[Indx[i]]);
	}

	return keyPointsSorted;
}

// This function focuses mainly one extreme spread among all available keypoints. 
// It should mainly be used on runtime-images, not on training images! 
// Its keypoints are not the best available in the image, but the good spread allows for a lot of matches.
void ImageComparator::detectAndComputeSpreadOut(Ptr<cv::Feature2D> orbGenerator, const cv::Mat& pic, InputArray mask, std::vector<KeyPoint>& query_keypointsFinal, OutputArray descriptors, int optionalMaxFeatureOverride) {
	std::vector<KeyPoint> query_keypoints = std::vector<KeyPoint>();
	orbGenerator->detect(pic, query_keypoints, mask);

	std::vector<KeyPoint> keyPointsSorted = sortKeypoints(query_keypoints);

	// get X amound of keypoints that are well spread over the area
	int maxKeypoints = staticNamespace::nfeatures;
	if (optionalMaxFeatureOverride != -1)
	{
		maxKeypoints = optionalMaxFeatureOverride;
	}
	float tolerance = 0.1; // tolerance of the number of return points

	algs::ssc(keyPointsSorted, query_keypointsFinal, maxKeypoints, tolerance, pic.cols, pic.rows);

	mBADDescriptor->compute(pic, query_keypointsFinal, descriptors);
}

// This function spreads keypoints over the area, but focuses on keeping the best keypoints available, even if that means clustering them in certain positions.
// It should be used for training images, providing a good solution to test against.
void ImageComparator::detectAndComputeSpreadOutUsingBuckets(Ptr<cv::Feature2D> orbGenerator, const cv::Mat& pic, InputArray mask, std::vector<KeyPoint>& query_keypointsFinal, OutputArray descriptors, int initialFeatureCount) {
	std::vector<KeyPoint> query_keypoints = std::vector<KeyPoint>();

	// detect a lot of keypoints
	orbGenerator->detect(pic, query_keypoints, mask);
	query_keypoints = sortKeypoints(query_keypoints);

	float bucketSize = bucket_size_multiplier_query;
	// make sure even if every bucket has a keypoint, the whole screen will be keypointed anyway
	float minimumBucketsize = 1.0f / sqrt(staticNamespace::nfeatures);
	if (bucketSize < minimumBucketsize)
	{
		bucketSize = minimumBucketsize;
	}

	std::map<unsigned int, std::vector<IndexResponseTupel>> bucketHashmap = std::map<unsigned int, std::vector<IndexResponseTupel>>();
	std::vector<IndexResponseTupel> leftovers = std::vector<IndexResponseTupel>();
	unsigned int idx = 0;
	int keypointsForcedPerBucket = 1; // this number of keypoints are forced per bucket. Default: 1. Higher than 1 seems to perform worse currently.
	for (auto& query_pnt : query_keypoints)
	{
		int bucketIndex = findAreaSpreadBucket(currentPictureSize, query_pnt.pt, bucketSize);
		if (!bucketHashmap.count(bucketIndex)) {
			bucketHashmap[bucketIndex] = std::vector<IndexResponseTupel>();
		}
		bucketHashmap[bucketIndex].push_back(IndexResponseTupel{ idx, query_pnt.response });
		idx++;
	}

	int maxKeypoints = initialFeatureCount;
	bool anyAddedKp = false;
	unsigned int alreadyAddedKp = 0;

	for (int i = 0; i < maxKeypoints; i++)
	{
		for (const auto& mapEntry : bucketHashmap)
		{
			if (mapEntry.second.size() > i)
			{
				int mappedIndex = mapEntry.second[i].index;

				if (i >= keypointsForcedPerBucket)
				{
					leftovers.push_back(IndexResponseTupel{ mapEntry.second[i].index, mapEntry.second[i].keypointResponse });
				}
				else {
					query_keypointsFinal.push_back(query_keypoints[mappedIndex]);
					anyAddedKp = true;
					alreadyAddedKp++;
					if (alreadyAddedKp >= maxKeypoints)
					{
						break;
					}
				}
			}
		}

		if ((!anyAddedKp && !(i >= keypointsForcedPerBucket)) || alreadyAddedKp >= maxKeypoints)
		{
			break;
		}
	}
	sort(leftovers.begin(), leftovers.end(), compareIndexResponseTupel);
	for (auto element : leftovers)
	{
		query_keypointsFinal.push_back(query_keypoints[element.index]);
		alreadyAddedKp++;
		if (alreadyAddedKp >= maxKeypoints)
		{
			break;
		}
	}
	query_keypointsFinal = sortKeypoints(query_keypointsFinal);

	mBADDescriptor->compute(pic, query_keypointsFinal, descriptors);
}


void ImageComparator::registerNewScan() {
	scanIteration++;
}

void ImageComparator::PostprocessDataVec(std::vector<ComparisonData>& dataVec, int finalPicWidth, int finalPicHeight) {
	if (finalPicWidth == 0 || finalPicHeight == 0) return;
	for (int i = 0; i < dataVec.size(); i++)
	{
		dataVec[i].position_original_card.x /= finalPicWidth;
		dataVec[i].position_original_card.y /= finalPicHeight;
		dataVec[i].scale_original_card *= KEYPOINT_GENEREATION_RESOLUTION / scanResolution;
	}
}

std::vector<KeyPoint> ImageComparator::compareImageDebug(cv::Mat& pic, std::vector<ComparisonData>& dataVec, bool singlePictureOnly /* = false*/) {
	std::vector<KeyPoint> query_keypoints = std::vector<KeyPoint>();
	Mat descriptors;

	// used for saving scans to Ram for future scanning of this exact image
	if (addNextImageWithName != "")
	{
		addImageWithPrimedName(pic);
	}

	registerNewScan();
	// pre-process pic NEEDS to happen BEFORE calculating resolutionDiagonal!!!! (because resultion might change)
	preprocessPicture(pic, false);

	double resolutionDiagonal = sqrt(pic.cols * pic.cols + pic.rows * pic.rows);
	currentPictureSize = cv::Point2i(pic.cols, pic.rows);

	initSearchMask(pic.cols, pic.rows, searchMaskSize);

	detectAndComputeSpreadOut(mOrbCompare, pic, searchMask, query_keypoints, descriptors);

	iterativeGroupBasedDetection(originGroups, query_keypoints, descriptors, resolutionDiagonal, dataVec, false, singlePictureOnly);
	PostprocessDataVec(dataVec, pic.cols, pic.rows);
	descriptors.release();

	return query_keypoints;
}

std::vector<KeyPoint> ImageComparator::compareImageBoard(cv::Mat& pic, std::vector<ComparisonData>& dataVec, bool singlePictureOnly /* = false*/, bool debugImageOutput /* = false*/) {

	std::vector<KeyPoint> query_keypoints_return_debug = std::vector<KeyPoint>();
	if (addNextImageWithName != "")
	{
		addImageWithPrimedName(pic);
	}

	registerNewScan();
	// pre-process pic NEEDS to happen BEFORE calculating resolutionDiagonal!!!! (because resultion might change)
	preprocessPicture(pic, false);

	// split image into several sub-images for fine-grain search of seperate sectors!
	int widthSplits = 8;
	int heightSplits = 6;
	int picWidth = pic.cols;
	int picHeight = pic.rows;
	float blockWidth = 2.0f * (float)picWidth / (float)(widthSplits + 1);
	float blockHeight = 2.0f * (float)picHeight / (float)(heightSplits + 1);
	int blockWidthInt = ceilf(blockWidth);
	int blockHeightInt = ceilf(blockHeight);
	for (int j = 0; j < widthSplits; j++) {
		for (int k = 0; k < heightSplits; k++) {
			std::vector<KeyPoint> query_keypoints = std::vector<KeyPoint>();
			Mat descriptors;

			int offsetX = (int)(j * blockWidth / 2.0f);
			int offsetY = (int)(k * blockHeight / 2.0f);

			Mat cropped_image = pic(Range(offsetY, offsetY + blockHeight - 1), Range(offsetX, offsetX + blockWidthInt - 1));

			double resolutionDiagonal = sqrt(cropped_image.cols * cropped_image.cols + cropped_image.rows * cropped_image.rows); //TagJK_revert!
			currentPictureSize = cv::Point2i(cropped_image.cols, cropped_image.rows);

			initSearchMask(cropped_image.cols, cropped_image.rows, searchMaskSize);

			detectAndComputeSpreadOut(mOrbCompare, cropped_image, searchMask, query_keypoints, descriptors);

			iterativeGroupBasedDetection(originGroups, query_keypoints, descriptors, resolutionDiagonal, dataVec, false, singlePictureOnly);
			PostprocessDataVec(dataVec, cropped_image.cols, cropped_image.rows);

			// for debug visualization, keep the query keypoints of the first successful result
			if (dataVec.size() > 0 && debugImageOutput && query_keypoints_return_debug.size() == 0)
			{
				query_keypoints_return_debug = query_keypoints;
			}
			descriptors.release();
			cropped_image.release();
		}
	}

	return query_keypoints_return_debug;
}

void ImageComparator::findClosestMatchFast(
	std::vector<unsigned int>& indicesAlreadyChecked,
	std::vector<unsigned int> originalIndicesToCheck,
	bool cancelOnFirstMatch,
	// data for "matchResults()" 
	std::vector<KeyPoint>& keypoints1 /*query pic, the one newly scanned*/,
	Mat& descriptors1,
	double resolutionDiagonal,
	int minimumMatches,
	int firstCheckFastCancel, // if this is anything but "-1" and we get a result on the first check, we cancel immideately! 
	std::vector<ComparisonData>& dataVec
) {
	// check only what we havent checked yet
	std::vector<unsigned int> indicesToCheck = std::vector<unsigned int>();
	for (auto idxToCheck : originalIndicesToCheck)
	{
		// if index not found in "indicesAlreadyChecked" ad it to "indicesToCheck"
		if (std::find(indicesAlreadyChecked.begin(), indicesAlreadyChecked.end(), idxToCheck) == indicesAlreadyChecked.end()) {
			indicesToCheck.push_back(idxToCheck);
		}
	}
	if (indicesToCheck.size() == 0) return;

	auto originalDataVecSize = dataVec.size();
	bool earlyCancel = false;
	for (unsigned int idxComparePic : indicesToCheck) {
		std::vector<ComparisonData> newDataVec = std::vector<ComparisonData>();
		matchResults(keypoints1, descriptors1, mKeypoints[idxComparePic], mDescriptors[idxComparePic], minimumMatches, mPicCenters[idxComparePic], newDataVec, idxComparePic, resolutionDiagonal, true);
		indicesAlreadyChecked.push_back(idxComparePic);
		// gather only best result if multiple of the same idxComparePic where found
		if (newDataVec.size() > 0)
		{
			std::sort(newDataVec.begin(), newDataVec.end(), greater<ComparisonData>());
			dataVec.push_back(newDataVec[0]);
			// if firstCheckFastCancel
			if (firstCheckFastCancel != -1)
			{
				if (newDataVec[0].match_rating >= firstCheckFastCancel)
				{
					// firstCheckFastCancel was successful. No further tests required!
					return;
				}
			}
		}
		// firstCheckFastCancel is only used on the very first check ever.
		firstCheckFastCancel = -1;

		// if we have a new result, check if we want to stop early
		if (dataVec.size() > originalDataVecSize && cancelOnFirstMatch /* && earlyCancelOnActiveCards*/) {
			earlyCancel = true;
			break;
		}
	}
	// check if found matches point towards similar pictures
	std::vector<unsigned int> newIndicesToCheck = std::vector<unsigned int>();
	for (size_t i = originalDataVecSize; i < dataVec.size(); i++)
	{
		unsigned int foundIdx = dataVec[i].id_original_card;
		auto& cardSimilarities = mCardSimilarities[foundIdx];
		for (auto cardId : cardSimilarities)
		{
			// only add new searchtarget if its NOT yet in the newIndicesToCheck vec AND if its part of the activeCards (dont search outside activeCards)
			if (std::find(newIndicesToCheck.begin(), newIndicesToCheck.end(), cardId) == newIndicesToCheck.end()
			 && std::find(originalIndicesToCheck.begin(), originalIndicesToCheck.end(), cardId) != originalIndicesToCheck.end()) {
				newIndicesToCheck.push_back(cardId);
			}
		}
		/*
		auto& keypointSimilaritySet = mKeypointSimilarities[foundIdx];
		for (auto match : dataVec[i].matches)
		{
			auto& specificKeypointSimilarities = keypointSimilaritySet[match.trainIdx];
			for (auto similarity : specificKeypointSimilarities) {
				if (std::find(newIndicesToCheck.begin(), newIndicesToCheck.end(), similarity.idSimilarKeypointSet) == newIndicesToCheck.end()) {
					newIndicesToCheck.push_back(similarity.idSimilarKeypointSet);
				}
			}
		}
		*/
	}
	// check these similar pictures as well (WITHOUT early cancel)
	findClosestMatchFast(indicesAlreadyChecked, newIndicesToCheck, false, keypoints1, descriptors1, resolutionDiagonal, minimumMatches, -1, dataVec);
}


cv::Mat ImageComparator::checkUniqueness(ComparisonData& data, bool draw_result) {
	size_t reference_id = data.id_original_card;
	auto &keypoints = mKeypoints[reference_id];
	auto &descriptors = mDescriptors[reference_id];
	auto &pic = mPics[reference_id];
	ComparisonData data_match = ComparisonData();
	ComparisonData best_match = ComparisonData();
	double resolutionDiagonal = sqrt(pic.cols * pic.cols + pic.rows * pic.rows);
	currentPictureSize = cv::Point2i(pic.cols, pic.rows);

	float best_match_rating = 0;
	int times_above_match_threshhold = 0;
	float uniqueness_overall = 0;
	size_t bestMatchIndex = 0;
	std::vector<DMatch> best_matches;
	for (size_t idxComparePic = 0; idxComparePic < mDescriptors.size(); idxComparePic++)
	{
		if (idxComparePic == reference_id) { continue; }

		auto &refKeypoints = mKeypoints[idxComparePic];
		auto &refDescriptors = mDescriptors[idxComparePic];
		std::vector<ComparisonData> newData;
		//float match_rating = matchResults(refKeypoints, refDescriptors, keypoints, descriptors, matches);
		matchResults(keypoints, descriptors, refKeypoints, refDescriptors, minimum_match_count, mPicCenters[idxComparePic], newData, (int)idxComparePic, resolutionDiagonal);
		if (newData.size() > 0)
		{
			data_match = newData[0];
		} else {
			data_match = ComparisonData();
		}
		uniqueness_overall += data_match.match_rating;
		if (minimum_match_rating < data_match.match_rating)
		{
			times_above_match_threshhold++;
		}
		if (best_match_rating < data_match.match_rating)
		{
			best_match_rating = data_match.match_rating;
			bestMatchIndex = idxComparePic;
			best_match = data_match;
		}
	}
	uniqueness_overall /= (mDescriptors.size() - 1);

	data.similarity_most_similar_to_original = best_match_rating;
	data.id_most_similar_to_original = (int)bestMatchIndex;
	data.uniqueness = 100 * uniqueness_overall;
	data.matches_found = best_match.matches_found;
	data.matches_final = best_match.matches_final;
	// Draw top matches
	Mat matchpic = Mat();
	//for (int i = 0; i < best_matches.size(); i++)
	//{
	//	// invert match-idices to show reference picture on left side
	//	auto tmp = best_matches[i].queryIdx;
	//	best_matches[i].queryIdx = best_matches[i].trainIdx;
	//	best_matches[i].trainIdx = tmp;
	//}
	bool security_checks = mPics.size() > bestMatchIndex && best_matches.size() > 0;
	if (draw_result && security_checks)
	{
		// compare picture returned just for debugging
		//drawMatches(mPics[bestMatchIndex], mKeypoints[bestMatchIndex], pic, keypoints, best_matches, imMatches);
		drawMatches(pic, keypoints, mPics[bestMatchIndex], mKeypoints[bestMatchIndex], best_matches, matchpic,
			Scalar::all(-1), Scalar::all(-1), std::vector<char>(), DrawMatchesFlags::DEFAULT); //DrawMatchesFlags::NOT_DRAW_SINGLE_POINTS | DrawMatchesFlags::DRAW_RICH_KEYPOINTS);
	}
	else
	{
		if (!security_checks)
		{
			matchpic = Mat(mPics[bestMatchIndex].rows, mPics[bestMatchIndex].cols * 2, mPics[bestMatchIndex].type(), Scalar(0, 0, 0));
		}
		else
		{
			return Mat();
		}
		
	}
	Mat resulting_pic(200, matchpic.cols, matchpic.type(), Scalar(0, 0, 0));
	resulting_pic.push_back(matchpic);

	return resulting_pic;
}
void ImageComparator::adaptiveThreshold(cv::Mat &mat) {
	cv::adaptiveThreshold(mat, mat, 255, cv::ADAPTIVE_THRESH_MEAN_C, cv::THRESH_BINARY_INV, 21, 5);
}


// compatibility function for Houston Dolphin code
const std::string& ImageComparator::imageDetection(cv::Mat& mat) {
	return detectSingleImageString(mat, false);
}
const std::string& ImageComparator::testGrayOutPic(cv::Mat& mat) {
	auto originalSize = cv::Size(mat.cols, mat.rows);
	cvtColor(mat, mat, cv::COLOR_RGBA2GRAY);
	//preprocessPicture(mat, false);
	cvtColor(mat, mat, cv::COLOR_GRAY2RGBA);
	//cv::resize(mat, mat, originalSize);
	mResultFileName = "";
	return mResultFileName;
}
const std::string& ImageComparator::detectSingleImageString(cv::Mat& mat, bool isColoredPicture, bool debugImageOutput /* = false*/) {
	m_data.clear();
	auto originalSize = cv::Size(mat.cols, mat.rows);
	if (isColoredPicture)
	{
		cvtColor(mat, mat, cv::COLOR_RGBA2GRAY); // preprocessing requires grey-images (for Histogram Equalization)
	}
	//using namespace cv;
	//cv::UMat umat = mat.getUMat(cv::ACCESS_RW);
	compareImageDebug(mat, m_data, true);


	mResultFileName = "";

	// build result-name from subresults (if applicable)
	ComparisonData* matchResultPtr = nullptr;
	if (m_data.size() > 0)
	{
		matchResultPtr = &m_data[0];
		mResultFileName += mFilenames[matchResultPtr->id_original_card];

		while (matchResultPtr->subResults.size() > 0)
		{
			matchResultPtr = &matchResultPtr->subResults[0];
			mResultFileName += mFilenames[matchResultPtr->id_original_card];
		}
	}

	const std::string* ret = &mResultFileName;


	if (debugImageOutput)
	{
		cv::resize(mat, mat, originalSize);
		cvtColor(mat, mat, cv::COLOR_GRAY2RGBA); // if image output is needed, revert back to RGB mat type for output purposes
	}
	return *ret;
}
void ImageComparator::detectSingleImage(cv::Mat& mat, bool isColoredPicture, bool debugImageOutput /* = false*/) {
	m_data.clear();
	auto originalSize = cv::Size(mat.cols, mat.rows);
	if (isColoredPicture)
	{
		cvtColor(mat, mat, cv::COLOR_RGBA2GRAY); // preprocessing requires grey-images (for Histogram Equalization)
	}
	//using namespace cv;
	//cv::UMat umat = mat.getUMat(cv::ACCESS_RW);
	storedQueryKeypoints = compareImageDebug(mat, m_data, true);

	if (debugImageOutput)
	{
		cv::resize(mat, mat, originalSize);
		cvtColor(mat, mat, cv::COLOR_GRAY2RGBA); // if image output is needed, revert back to RGB mat type for output purposes
	}
}
const void ImageComparator::removePerspective(cv::Mat& mat, float heightMult, float widthMult) {
	float dynamicHeightOffset = 0.3f; // a value between 0.0f and 1.0f-heightMultiplier that shifts the 
									  // visible perspective-corrected Area up and down within the cameras view
	float heightMultiplier = heightMult; //= 0.3f;
	float widthMultiplier = widthMult; //= 0.65f;

	float targetWidth = (float)mat.cols;
	float targetHeight = (float)mat.rows;
	float angleBasedWidth = targetWidth * widthMultiplier;
	float angleBasedHeight = targetHeight * heightMultiplier;
	float widthOffset = (targetWidth - angleBasedWidth) / 2.0f;
	float heightOffset = targetHeight - angleBasedHeight - (dynamicHeightOffset * targetHeight);
	//cv::Point2f originArea[4] = { 
	//		cv::Point2f(widhtOffset, 0),
	//		cv::Point2f(widhtOffset + angleBasedWidth, 0),
	//		cv::Point2f(0, angleBasedHeight),
	//		cv::Point2f(targetWidth - 1, angleBasedHeight),
	//};

	cv::Point2f originArea[4] = {
		cv::Point2f(0, heightOffset),
		cv::Point2f(targetWidth - 1, heightOffset),
		cv::Point2f(widthOffset, heightOffset + angleBasedHeight),
		cv::Point2f(widthOffset + angleBasedWidth, heightOffset + angleBasedHeight),
	};
	cv::Point2f destinationArea[4] = {
		cv::Point2f(0, 0),
		cv::Point2f(targetWidth - 1, 0),
		cv::Point2f(0, targetHeight - 1),
		cv::Point2f(targetWidth - 1, targetHeight - 1),
	};

	Mat perspectiveMatrix = cv::getPerspectiveTransform(originArea, destinationArea);
	cv::warpPerspective(mat, mat, perspectiveMatrix, cv::Size((int)targetWidth, (int)targetHeight));
}
const void ImageComparator::boardDetection(cv::Mat& mat, float heightMult, bool debugImageOutput = false) {
	cvtColor(mat, mat, cv::COLOR_RGBA2GRAY); // preprocessing requires grey-images (for Histogram Equalization)

	// just for testing
	m_data.clear();
	storedQueryKeypoints = compareImageDebug(mat, m_data, false);//compareImage(newMat);

	if (debugImageOutput)
	{
		cvtColor(mat, mat, cv::COLOR_GRAY2RGBA); // if image output is needed, revert back to RGB mat type for output purposes
	}
	/*



	float heightIncr = 0.0111;
	float currentHeightMult = 0;
	float currentWidthMult = 0;
	Mat newMat;

	heightMult = m_currPerspectiveMult;

	std::vector<ComparisonData> perspectiveChangeData = std::vector<ComparisonData>();
	m_data.clear();
	int maxTries = 10;
	bool checkPerspective = false;
	for (int i = 1; i < maxTries; i++)
	{
		currentHeightMult = heightMult + heightIncr * (float)(i / 2) * (float)(1.0f - 2.0f * (i % 2));
		currentWidthMult = currentHeightMult;

		mat.copyTo(newMat);
		removePerspective(newMat, currentHeightMult, currentWidthMult);
		std::vector<KeyPoint> queryKeypoints = compareImageDebug(newMat, m_data);//compareImage(newMat);

		if (m_data.size() > 0)
		{
			if (m_perspectiveConfidence == 0 || i > 2)
			{
				m_currPerspectiveMult = currentHeightMult;
				m_perspectiveConfidence = 0.1;
			}
			// do a perspective correction check every 5 scans
			if (m_timesSinceLastPerspectiveCheck > 3 && i == 1)
			{
				checkPerspective = true;
				m_timesSinceLastPerspectiveCheck = 0;
			};
			storedQueryKeypoints = queryKeypoints;
			break;
		}
	}
	if (checkPerspective)
	{
		Mat newAlternativeMat;
		currentHeightMult = currentHeightMult + m_perspectiveCheckDirection *  0.2f * heightIncr;
		m_perspectiveCheckDirection *= -1;
		currentWidthMult = currentHeightMult;

		mat.copyTo(newAlternativeMat);
		removePerspective(newAlternativeMat, currentHeightMult, currentWidthMult);
		std::vector<KeyPoint> queryKeypoints = compareImageDebug(newAlternativeMat, perspectiveChangeData);//compareImage(newMat);

		// if on a correction check the second test is better than the first, shift m_currPerspectiveMult over to the new perspective
		float combinedMatchRatingSaved = 0;
		float combinedMatchRatingCurrent = 0;
		for (ComparisonData& data : perspectiveChangeData) { combinedMatchRatingCurrent += data.match_rating; };
		for (ComparisonData& data : m_data) { combinedMatchRatingSaved += data.match_rating; };

		if ((perspectiveChangeData.size() > m_data.size()) || (m_data.size() == perspectiveChangeData.size() && combinedMatchRatingCurrent > combinedMatchRatingSaved))
		{
			float tempPerspective = (m_currPerspectiveMult + currentHeightMult) / 2.0f;
			m_currPerspectiveMult = tempPerspective;
			newMat = newAlternativeMat;
			// overwrite currently saved data
			m_data = perspectiveChangeData;
			storedQueryKeypoints = queryKeypoints;
		}
		else {
			// otherwise keep original result data
		}
	}

	if (m_data.size() == 0)
	{
		m_perspectiveConfidence -= 0.01;
		if (m_perspectiveConfidence < 0)
			m_perspectiveConfidence = 0;
	} else {
		m_timesSinceLastPerspectiveCheck++;
	}

	if (debugImageOutput)
	{
		if (m_data.size() != 0)
		{
			cvtColor(newMat, mat, cv::COLOR_GRAY2RGBA); // if image output is needed, revert back to RGB mat type for output purposes
		} else {
			cvtColor(mat, mat, cv::COLOR_GRAY2RGBA); // if image output is needed, revert back to RGB mat type for output purposes
		}
	}
	*/

	/*
	cvtColor(mat, mat, cv::COLOR_RGBA2GRAY); // preprocessing requires grey-images (for Histogram Equalization)
	std::string emptyString = "";
	std::string& ret = emptyString;
	float heightIncr = 0.05;
	float WidthIncr = 0.05;
	float currentHeightMult = 0;
	float currentWidthMult = 0;
	Mat newMat;
	for (int i = 0; i < 1; i++)
	{
		currentHeightMult = heightMult + heightIncr*(float)(i / 2)*(float)(1.0f - 2.0f * (i % 2));
		for (int j = 0; j < 1; j++)
		{
			currentWidthMult = widthMult + WidthIncr * (float)(j / 2) * (float)(1.0f - 2.0f * (j % 2));
			mat.copyTo(newMat);
			removePerspective(newMat, currentHeightMult, currentWidthMult);
			ret = compareImage(newMat);

			if (ret != "")
				break;
		}
		if (ret != "")
			break;
	}
	if (debugImageOutput)
	{
		cvtColor(newMat, mat, cv::COLOR_GRAY2RGBA); // if image output is needed, revert back to RGB mat type for output purposes
	}
	return ret;
	
	// old values for 30°
	//widthmult: 0.555
	//heightmult: 0.33
	*/
}

// compatibility function for Houston Dolphin code
long ImageComparator::exchangeData(long dataType, long data) {
	long returnVal = 0;

	std::string cardFilter = "";
	std::string cardFilter2 = "";
	if (data > groupNames.size() || data < 0)
	{
		return returnVal;
	}

	if (data == 0) {
		cardFilter = "teamCards";
		cardFilter2 = "teamCardsFrench";
	}
	if (data == 1) cardFilter = "normalCards";

	switch (dataType)
	{
	case DATA_EXCHANGE_TYPE::resetCardFilters: {
		// this is used to change what cards are available to scan
		returnVal = setCardFilter(FILTER_MODE::ResetFilters);
	} break; case DATA_EXCHANGE_TYPE::addCardGroupAllowance: {
		returnVal = setCardFilter(FILTER_MODE::AddAllowance, cardFilter);
		if (cardFilter2 != "")
		{
			setCardFilter(FILTER_MODE::AddAllowance, cardFilter2);
		}
	} break; case DATA_EXCHANGE_TYPE::addCardGroupDisallowance: {
		returnVal = setCardFilter(FILTER_MODE::AddDisallowance, cardFilter);
	} break; default: {}
	}
	return returnVal;
}

long ImageComparator::callCommand(std::string commandType, std::string value) {

	long returnVal = 0;
	
	if (commandType == "resetCardFilters") {
		// this is used to change what cards are available to scan
		returnVal = setCardFilter(FILTER_MODE::ResetFilters);
	}
	if (commandType == "addCardGroupAllowance") {
		returnVal = setCardFilter(FILTER_MODE::AddAllowance, value);
	}
	if (commandType == "addCardGroupDisallowance") {
		returnVal = setCardFilter(FILTER_MODE::AddDisallowance, value);
	}
	if (commandType == "addNextPictureToScannables") {
		addNextPictureToScannables(value, false);
	}
	if (commandType == "removeScannableByIndex") {
		removeScannableByIndex(std::stoi(value));
	}
	if (commandType == "removeScannableFromBack") {
		removeScannableFromBack();
	}
	return returnVal;
}

const std::vector<std::vector<cv::KeyPoint>>& ImageComparator::getKeypointVectors()
{
	return mKeypoints;
}

const std::vector<cv::Mat>& ImageComparator::getPictures()
{
	return mPics;
}
const cv::Mat& ImageComparator::getPictureOfIndex(unsigned int index)
{
	if (mPics.size() > index)
	{
		return mPics[index];
	} else {
		return emptyPic;
	}
}

void ImageComparator::updateEnabledKeypoints(size_t card_id, const cv::Mat& disabled_areas)
{
	// make sure keypoints are backed up
	if (!hasKeypointBackup())
		createKeypointBackup();

	// start keypoint filtering
	auto & keypointVec = mBackupKeypoints[card_id];
	auto & targetKeypointVec = mKeypoints[card_id];
	auto & descriptorMat = mBackupDescriptors[card_id];
	auto & targetDescriptorMat = mDescriptors[card_id];
	size_t keypointVecSize = keypointVec.size();
	targetDescriptorMat = Mat();
	targetKeypointVec.clear();
	for (int idx = 0; idx < keypointVecSize; idx++)
	{
		auto & keypoint = keypointVec[idx];
		const unsigned char & value = disabled_areas.at<unsigned char>((int)keypoint.pt.y, (int)keypoint.pt.x);
		if (value == 0) {
			// keypoint NOT disabled
			targetKeypointVec.push_back(keypoint);
			targetDescriptorMat.push_back(descriptorMat.row(idx));
		} else {
			// keypoint disabled
		}
	}
}

bool ImageComparator::hasKeypointBackup()
{
	return mKeypoints.size() == mBackupKeypoints.size();
}
void ImageComparator::createKeypointBackup()
{
	for (auto &keypointVec : mKeypoints)
	{
		mBackupKeypoints.push_back(std::vector<cv::KeyPoint>());
		auto &currentVec = mBackupKeypoints.back();
		for (auto &keypoint : keypointVec)
		{
			currentVec.push_back(keypoint);
		}
	}	
	for (auto &descriptorMat : mDescriptors)
	{
		Mat newMat;
		descriptorMat.copyTo(newMat);
		mBackupDescriptors.push_back(newMat);
	}
}

void ImageComparator::checkUniquenessOfAll()
{

	FileStorage fs("uniqueness.xml", FileStorage::WRITE);

	ComparisonData data = ComparisonData();
	std::vector<ComparisonData> data_vec;
	for (size_t idxComparePic = 0; idxComparePic < mDescriptors.size(); idxComparePic++)
	{
		data.id_original_card = (int)idxComparePic;
		checkUniqueness(data);
		data_vec.push_back(data);

	}
	std::sort(data_vec.begin(), data_vec.end(), greater<ComparisonData>());

	for (size_t idx = 0; idx < data_vec.size(); idx++)
	{
		cv::write(fs, "______________", "_");
		cv::write(fs, "Card_id", mFilenames[data_vec[idx].id_original_card]);
		cv::write(fs, "similarity", std::to_string(data_vec[idx].similarity_most_similar_to_original));
		cv::write(fs, "most_similar_id", mFilenames[data_vec[idx].id_most_similar_to_original]);
		cv::write(fs, "uniqueness", std::to_string(data_vec[idx].uniqueness));
	}

	fs.release();
}

void ImageComparator::readFileColorsAndAlphaMask(const String& final_path, std::vector<Mat>& pics, std::vector<Mat>& masks) {
	cv::Mat loaded_pic = cv::imread(final_path, cv::IMREAD_GRAYSCALE);
	cv::Mat loaded_pic_with_alpha = cv::imread(final_path, cv::IMREAD_UNCHANGED);
	vector<Mat> layers;
	split(loaded_pic_with_alpha, layers); // seperate channels
	//Mat rgb[3] = { layers[0],layers[1],layers[2] };
	Mat mask = Mat();
	// if we have an alpha channel, use it as mask
	if (layers.size() > 3)
	{
		mask = layers[3];
		mask.setTo(0, mask < 128);
	}
	//mask = - 128;
	//mask *= 255.0;
	//min(mask, 0.0f);
	//mask -= 128;
	//mask *= 2;
	//mask.setTo(255, mask > 127);
	//cv::threshold(mask, mask, 128, 0, cv::THRESH_BINARY_INV);

	pics.push_back(loaded_pic);
	masks.push_back(mask);
	loaded_pic_with_alpha.release();
}


// init from a list of image paths
void ImageComparator::initPicturesOnly(string path, std::vector<string> allFiles, std::string groupname, std::vector<std::string> childGroups)
{
	auto simplevec = std::vector<Mat>();
	auto keypointMasks = std::vector<Mat>();

	// get current card-pool-size and use it as initial cardIndex for new cards
	// read in all files
	for (std::string filename : allFiles)
	{
		std::string final_path = path + "\\" + filename;
		//loaded_pic = cv::imread(final_path, cv::IMREAD_GRAYSCALE);
		readFileColorsAndAlphaMask(final_path, simplevec, keypointMasks);
	}
	// create keypoints using this read data and their alpha masks
	initDebugPicturesOnly(simplevec);
}

// init from a list of image paths
size_t ImageComparator::initFromPicturesWithGroups(string path, std::vector<string> allFiles, std::vector<string> allFileNames, std::string groupname, std::vector<std::string> childGroups, bool safePicData)
{
	auto simplevec = std::vector<Mat>();
	auto keypointMasks = std::vector<Mat>();
	std::vector<std::string> cardNames = std::vector<std::string>();
	std::vector<unsigned int> cardIndices = std::vector<unsigned int>();
	cv::Mat loaded_pic;

	// get current card-pool-size and use it as initial cardIndex for new cards
	size_t cardIndex = mKeypoints.size();
	// read in all files
	for (int idx = 0; idx < allFiles.size(); idx++)
	{
		std::string final_path = path + "\\" + allFiles[idx];

		cardIndices.push_back(cardIndex);
		cardIndex++;
		//loaded_pic = cv::imread(final_path, cv::IMREAD_GRAYSCALE);
		readFileColorsAndAlphaMask(final_path, simplevec, keypointMasks);

		addName(allFileNames[idx]);
	}
	// create keypoints using this read data and their alpha masks
	initWithPictureList(simplevec, keypointMasks, safePicData);

	// create card group
	addCardGroup(groupname, cardIndices, childGroups, true);

	// set filter to current card group and check uniqueness against eachother
	setCardFilter(FILTER_MODE::ResetFilters);
	setCardFilter(FILTER_MODE::AddAllowance, groupname);
	checkUniqueKeypoints(activeCards);
	setCardFilter(FILTER_MODE::ResetFilters); // reset filters again to make sure we dont have any filters on after loading

	return simplevec.size();
}
// init from a list of image paths
size_t ImageComparator::initFromPictures(const char *pathUtf, char * allFiles[], int amountOfFiles, bool safePicData)
{
	std::vector<std::string> fileNames = std::vector<std::string>();

	for (int i = 0; i < amountOfFiles; i++)
	{
		fileNames.push_back(string(allFiles[i]));
	}

	std::string path(pathUtf);
	// first group is the origin folder. It has no actual "childGroups" and is always called "origin"
	return initFromPicturesWithGroups(path, fileNames, fileNames, "origin", std::vector<std::string>(), safePicData);



	
	/*
	auto simplevec = std::vector<Mat>();
	auto keypointMasks = std::vector<Mat>();
	cv::Mat loaded_pic;

	for (int i = 0; i < amountOfFiles; i++)
	{
		string filename(allFiles[i]);
		std::string final_path = path + "/" +  filename;
		//loaded_pic = cv::imread(final_path, cv::IMREAD_GRAYSCALE);
		readFileColorsAndAlphaMask(final_path, simplevec, keypointMasks);

		addName(filename);
	}

	//// init from pictures directly
	////loaded_pic = cv::imread("images/wag0m.png", cv::IMREAD_GRAYSCALE);
	////simplevec.push_back(loaded_pic);
	//std::string path = "./images";
	//for (const auto & entry : fs::directory_iterator(path)) {
	//	auto tata = entry.path().string();
	//	if (entry.is_regular_file()) {
	//		loaded_pic = cv::imread(entry.path().string(), cv::IMREAD_GRAYSCALE);
	//		simplevec.push_back(loaded_pic);
	//		comparator.addName(entry.path().stem().string());
	//	}
	//}

	initWithPictureList(simplevec, keypointMasks);

	//TagJK_rethink this would always do the unique check with the default card filters. 
	checkUniqueKeypoints(activeCards);

	return simplevec.size();
	*/
}

void ImageComparator::safeLocalDescriptors()
{
	FileStorage fs("Keypoints.xml", FileStorage::WRITE);

	// save scanning resolution
	writeScanningResolution(fs);

	// save groups
	cv::write(fs, "groupNames", groupNames);
	for (size_t idx = 0; idx < groupNames.size(); idx++)
	{
		std::string currentGroupName = groupNames[idx];

		cv::write(fs, "childrenGroups_" + std::to_string(idx), childrenGroups[currentGroupName]);
		cv::write(fs, "cardGroupEarlyCancelAllowance_" + std::to_string(idx), cardGroupEarlyCancelAllowance[currentGroupName]);

		std::vector<int> cardIndexGroupsCONVERTEDtoInt = std::vector<int>();
		for (auto card : cardIndexGroups[currentGroupName])
		{
			cardIndexGroupsCONVERTEDtoInt.push_back(card);
		}
		cv::write(fs, "cardgroupByIndex_" + std::to_string(idx), cardIndexGroupsCONVERTEDtoInt);
	}

	// save cards and keypoints
	cv::write(fs, "card_count", std::to_string(mKeypoints.size()));
	for (size_t idx = 0; idx < mKeypoints.size(); idx++)
	{
		cv::write(fs, "name_" + std::to_string(idx), mFilenames[idx]);
		cv::write(fs, "keypoints_" + std::to_string(idx), mKeypoints[idx]);
		cv::write(fs, "descriptors_" + std::to_string(idx), mDescriptors[idx]);
		cv::write(fs, "center_" + std::to_string(idx), mPicCenters[idx]);
		// safe similarity vec as DMatch vector, because cv:read can only deal with vector::DMatch && vector:Keypoint
		std::vector<DMatch> similarityPackedInDmatches;
		for (unsigned int similarity : mCardSimilarities[idx])
		{
			DMatch dmatch;
			dmatch.imgIdx = (int)similarity;
			similarityPackedInDmatches.push_back(dmatch);
		}
		cv::write(fs, "similarities_" + std::to_string(idx), similarityPackedInDmatches);
	}
	fs.release();
	remove("Keypoints.jpg");
	remove("Keypoints.txt");
	rename("Keypoints.xml", "Keypoints.txt");
}
void ImageComparator::initSettingsFromFile(const char* utfString)
{
	std::string path(utfString);
	path = path + "/_folderSettings.txt";
	FileStorage fs(path, FileStorage::READ);
	initScanningResolution(fs);
	fs.release();
}
void ImageComparator::initScanningResolution(FileStorage& fs) {
	// STANDARDIZED_RESOLUTION and KEYPOINT_GENEREATION_RESOLUTION are taken from a settings file in the folder
	int scanningResolution = 0;
	int generationResolution = 0;
	int new_positionSameAsFirst = 1; // values 0 or 1 allowed, gets converted into bool
	int new_rotationSameAsFirst = 1; // values 0 or 1 allowed, gets converted into bool
	int new_scaleSameAsFirst = 1; // values 0 or 1 allowed, gets converted into bool
	fs["scanningResolution"] >> scanningResolution;
	fs["generationResolution"] >> generationResolution;
	if (!fs["positionSameAsFirst"].empty()) fs["positionSameAsFirst"] >> new_positionSameAsFirst;
	if (!fs["rotationSameAsFirst"].empty()) fs["rotationSameAsFirst"] >> new_rotationSameAsFirst;
	if (!fs["scaleSameAsFirst"].empty()) fs["scaleSameAsFirst"] >> new_scaleSameAsFirst;
	if (scanningResolution != 0) {
		STANDARDIZED_RESOLUTION = scanningResolution;
		this->scanResolution = scanningResolution;
		this->positionSameAsFirst = (bool)new_positionSameAsFirst;
		this->rotationSameAsFirst = (bool)new_rotationSameAsFirst;
		this->scaleSameAsFirst = (bool)new_scaleSameAsFirst;
	}
	if (generationResolution != 0) KEYPOINT_GENEREATION_RESOLUTION = generationResolution;
}
void ImageComparator::writeScanningResolution(FileStorage& fs) {
	// STANDARDIZED_RESOLUTION and KEYPOINT_GENEREATION_RESOLUTION are taken from a settings file in the folder (or using stardart values)
	// and write those settings into the keypoints-file as well, to load them again in project
	cv::write(fs, "scanningResolution", STANDARDIZED_RESOLUTION);
	cv::write(fs, "generationResolution", KEYPOINT_GENEREATION_RESOLUTION);
	cv::write(fs, "positionSameAsFirst", positionSameAsFirst);
	cv::write(fs, "rotationSameAsFirst", rotationSameAsFirst);
	cv::write(fs, "scaleSameAsFirst", scaleSameAsFirst);
}
// init from a prepared keypointfile
int ImageComparator::initFromFile(const char *utfString)
{
	std::string path(utfString);
	FileStorage fs(path, FileStorage::READ);

	initScanningResolution(fs);

	groupNames = std::vector<std::string>();
	cardIndexGroups = std::map<string, std::vector<unsigned int>>();
	childrenGroups = std::map<string, std::vector<std::string>>();
	recentResults = std::map<string, RecentScan>();
	cardGroupEarlyCancelAllowance = std::map<string, bool>();

	std::vector<std::string > readGroupNames;
	// load groups
	fs["groupNames"] >> readGroupNames;
	for (size_t idx = 0; idx < readGroupNames.size(); idx++)
	{
		std::string currentGroupName = readGroupNames[idx];
		std::vector<std::string> childrenGroup;
		bool cardGroupAllowsEarlyCancel = true;

		fs["childrenGroups_" + std::to_string(idx)] >> childrenGroup;
		//fs["cardGroupEarlyCancelAllowance_" + std::to_string(idx)] >> cardGroupAllowsEarlyCancel;

		std::vector<int> cardIndexGroupsCONVERTEDtoInt = std::vector<int>();
		fs["cardgroupByIndex_" + std::to_string(idx)] >> cardIndexGroupsCONVERTEDtoInt;
		std::vector<unsigned int> cardIndexGroup = std::vector<unsigned int>();
		for (auto card : cardIndexGroupsCONVERTEDtoInt)
		{
			cardIndexGroup.push_back(card);
		}

		addCardGroup(currentGroupName, cardIndexGroup, childrenGroup, cardGroupAllowsEarlyCancel);
	}

	// load cards and keypoints
	std::string amount = "";
	fs["card_count"] >> amount;
	int card_amount = std::stoi(amount);
	mFilenames.clear();
	for (size_t idx = 0; idx < card_amount; idx++)
	{
		cv::Mat descriptors;
		std::vector<cv::KeyPoint> keypoints;
		cv::Point2f picture_center;
		std::string name;
		std::vector<unsigned int> similarities;
		fs["name_" + std::to_string(idx)] >> name;
		mFilenames.push_back(name);
		fs["keypoints_" + std::to_string(idx)] >> keypoints;
		mKeypoints.push_back(keypoints);
		fs["descriptors_" + std::to_string(idx)] >> descriptors;
		mDescriptors.push_back(descriptors);
		fs["center_" + std::to_string(idx)] >> picture_center;
		mPicCenters.push_back(picture_center);
		// calculate buckets
		currentPictureSize = cv::Point2i(2.0f * picture_center.x, 2.0f * picture_center.y);
		calculateRequiredBuckets(mKeypoints.back());
		// read similarity vec as DMatch vector, because cv:read can only deal with vector::DMatch && vector:Keypoint
		std::vector<DMatch> similarityPackedInDmatches;
		fs["similarities_" + std::to_string(idx)] >> similarityPackedInDmatches;
		for (auto& fakeSimilarity : similarityPackedInDmatches)
		{
			similarities.push_back(fakeSimilarity.imgIdx);
		}
		mCardSimilarities.push_back(similarities);
	}
	fs.release();

	// this always needs to happen in the end (even on non-hardcoded cardGroups) to initialize activeCards
	recalculateFilteredCards();
	return card_amount;
}

void imageDetectionMat(cv::Mat &mat) {

    cv::adaptiveThreshold(mat, mat, 255, cv::ADAPTIVE_THRESH_MEAN_C, cv::THRESH_BINARY_INV, 21, 5);
}

/*
	// Homography could be used to correct the rotation of one of
	// the images, but thats not our objective right now...
	// Find homography
	h = findHomography(points1, points2, RANSAC);

	// Use homography to warp image
	warpPerspective(im1, im1Reg, h, im2.size());


// takes a match-vector and two indices
// checks their relative distances in the trainvector and the queryvector
// returns relative length (distance between trainpoins divided by distance of query points)
fun relativeDistances(matchvec: opencv_core.DMatchVector, idx_one : Long, idx_two : Long, train_KP : opencv_core.KeyPointVector) :Float{
	val pnt_train1 = train_KP[matchvec[idx_one].trainIdx().toLong()].pt()
	val pnt_query1 = mKeypoints[matchvec[idx_one].queryIdx().toLong()].pt()

	val pnt_train2 = train_KP[matchvec[idx_two].trainIdx().toLong()].pt()
	val pnt_query2 = mKeypoints[matchvec[idx_two].queryIdx().toLong()].pt()
	val dist_vec_train = sqrt((pnt_train2.x() - pnt_train1.x()).pow(2) + (pnt_train2.y() - pnt_train1.y()).pow(2))
	val dist_vec_query = sqrt((pnt_query2.x() - pnt_query1.x()).pow(2) + (pnt_query2.y() - pnt_query1.y()).pow(2))
	return (dist_vec_train / dist_vec_query)
}
fun  getBestmatchesSorted(matchvec: opencv_core.DMatchVector, _max_results : Int) :opencv_core.DMatchVector{
	// implement sorting for DMatchVector
	var max_results = _max_results
	if (matchvec.size() < max_results) {
		max_results = matchvec.size().toInt()
	}
	val list = ArrayList<IndexDistanceObj>()
	for (i in 0 until matchvec.size()) {
		list.add(IndexDistanceObj(i, matchvec[i].distance()))
	}
	list.sortedWith(compareBy(IndexDistanceObj::distance))
	val good_matches = opencv_core.DMatchVector()
	for (i in 0 until max_results) {
		good_matches.push_back(matchvec[list[i].idx])
	}
	return good_matches
}
*/

/*


	// Comparison code
	var matchvec = opencv_core.DMatchVector()
	matcher.match(mDescriptors, mDESCcard, matchvec)

	var test2 = 0.toFloat()
	val minimal_matches = 17 // should not be lower than 3
	var good_matches = getBestmatchesSorted(matchvec, minimal_matches)
	if (matchvec.size() >= minimal_matches) {
		var overall_distance_multipliers = 0.toFloat()
			for (i in 0 until good_matches.size()) {
				var compare_index_1 = i + 1
					var compare_index_5 = i + 5
					if (i + 1 >= good_matches.size()) {
						compare_index_1 = i + 1 - good_matches.size()
					}
				if (i + 5 >= good_matches.size()) {
					compare_index_5 = i + 5 - good_matches.size()
				}
				overall_distance_multipliers += (relativeDistances(good_matches, i, compare_index_1, mKPcard))
					overall_distance_multipliers += (relativeDistances(good_matches, i, compare_index_5, mKPcard))
			}
		val average_distance_multipliers = overall_distance_multipliers / (2 * good_matches.size())
			var overall_deviation = 0.toFloat()
			for (i in 0 until good_matches.size()) {
				var compare_index_1 = i + 1
					var compare_index_5 = i + 5
					if (i + 1 >= good_matches.size()) {
						compare_index_1 = i + 1 - good_matches.size()
					}
				if (i + 5 >= good_matches.size()) {
					compare_index_5 = i + 5 - good_matches.size()
				}
				val fucking_write_that_index_already_asshole = 0
					overall_deviation += (average_distance_multipliers - (relativeDistances(good_matches, i, compare_index_1, mKPcard))).absoluteValue
					overall_deviation += (average_distance_multipliers - (relativeDistances(good_matches, i, compare_index_5, mKPcard))).absoluteValue
			}
		val average_deviation = overall_deviation / (2 * good_matches.size())

			test2 = average_deviation
	}

	//prozentualize
	var temp = (1 - (test2 - 1) / 5)
		if (temp < 0) {
			temp = 0.toFloat()
		}
	test2 = 100 * temp

*/


using namespace std;

#define CLUSTER_EPSILON 1.2 // originally 0.5 but it seemed too low. Whats the point of multiple "found" centers this close to eachother?

#define cast_uint32_t static_cast<uint32_t>
static inline float
fastpow2(float p)
{
	float offset = (p < 0) ? 1.0f : 0.0f;
	float clipp = (p < -126) ? -126.0f : p;
	int w = clipp;
	float z = clipp - w + offset;
	union { uint32_t i; float f; } v = { cast_uint32_t((1 << 23) * (clipp + 121.2740575f + 27.7280233f / (4.84252568f - z) - 1.49012907f * z)) };

	return v.f;
}

static inline float
fastexp(float p)
{
	return fastpow2(1.442695040f * p);
}

static inline float
fasterpow2(float p)
{
	float clipp = (p < -126) ? -126.0f : p;
	union { uint32_t i; float f; } v = { cast_uint32_t((1 << 23) * (clipp + 126.94269504f)) };
	return v.f;
}

static inline float
fasterexp(float p)
{
	return fasterpow2(1.442695040f * p);
}

inline float euclidean_distance(const Point2f& point_a, const Point2f& point_b) {
	return sqrt((point_a.x - point_b.x) * (point_a.x - point_b.x) + (point_a.y - point_b.y) * (point_a.y - point_b.y));
}

inline float euclidean_distance_sqr(const Point2f& point_a, const Point2f& point_b) {
	return ((point_a.x - point_b.x) * (point_a.x - point_b.x) + (point_a.y - point_b.y) * (point_a.y - point_b.y));
}

inline float gaussian_kernel(float distance_sqr, float kernel_bandwidth_gaussprepaired) {
	if (distance_sqr == 0) return 1;
	// original code looked as follows (but was replaced by precalculating kernel_bandwidth)
	//float temp2 = exp(-1.0f / 2.0f * (distance_sqr) / (kernel_bandwidth_gaussprepaired));
	float temp = fasterexp(distance_sqr / kernel_bandwidth_gaussprepaired);
	return temp;
}

inline int MeanShift::shift_point(const Point2f& point,
	const std::vector<Point2f>& points,
	float kernel_bandwidth_gaussprepaired,
	Point2f& shifted_point) {
	shifted_point.x = 0;
	shifted_point.y = 0;
	float total_weight = 0;
	int closePoints = 0;
	for (int i = 0; i < points.size(); i++) {
		const Point2f temp_point = points[i];
		float distance_sqr = euclidean_distance_sqr(point, temp_point);
		float weight = gaussian_kernel(distance_sqr, kernel_bandwidth_gaussprepaired);
		// count how many points are closer to the point than "kernel_bandwidth"
		if (weight > 0.6065306f)
		{
			closePoints++;
		}
		shifted_point.x += temp_point.x * weight;
		shifted_point.y += temp_point.y * weight;
		total_weight += weight;
	}
	shifted_point /= total_weight;
	return closePoints;
}

// algorithm for finding clusters within a cloud of points. 
// Used in this context to find the most likely center of the card (the point most keypoints would see as their card center)
std::vector<Point2f> MeanShift::meanshift(const std::vector<Point2f>& points, float kernel_bandwidth, float EPSILON, int minimumPointsPerCluster) {
	const float EPSILON_SQR = EPSILON * EPSILON;
	float kernel_bandwidth_gaussprepaired = -2.0f * kernel_bandwidth * kernel_bandwidth;
	// seed points
	vector<Point2f> shifted_points = points;// implement a grid of initial center points, instead of using all points as start
	vector<int> stop_moving(shifted_points.size(), 0);


	float max_shift_distance;
	Point2f point_new = Point2f(0,0);
	do {
		max_shift_distance = 0;
		for (int i = 0; i < points.size(); i++) {
			if (!stop_moving[i]) {
				int closePoints = shift_point(shifted_points[i], points, kernel_bandwidth_gaussprepaired, point_new);
				float shift_distance_sqr = euclidean_distance_sqr(point_new, shifted_points[i]);
				if (shift_distance_sqr > max_shift_distance) {
					max_shift_distance = shift_distance_sqr;
				}
				if (shift_distance_sqr <= EPSILON_SQR) {
					if (closePoints >= minimumPointsPerCluster)
						stop_moving[i] = 1;
					else 
						stop_moving[i] = 2; // this center has less than x keypoints nearby. disregard it later
				}
				shifted_points[i] = point_new;
			}
		}
		//printf("max_shift_distance: %f\n", sqrt(max_shift_distance));
	} while (max_shift_distance > EPSILON_SQR);
	
	// find cluster centers
	vector<Point2f> finalCenters = vector<Point2f>();
	for (int i = 0; i < shifted_points.size(); i++) {
		if (stop_moving[i] == 2)
		{
			// if this stopped with less that 4 keypoints close by, disregard it
			continue;
		}
		int c = 0;
		for (; c < finalCenters.size(); c++) {
			if (euclidean_distance(shifted_points[i], finalCenters[c]) <= CLUSTER_EPSILON) {
				break;
			}
		}
		// if not close to any known center, add new center
		if (c == finalCenters.size()) {
			finalCenters.push_back(shifted_points[i]);
		}
	}

	return finalCenters;// finalCenters;
}
