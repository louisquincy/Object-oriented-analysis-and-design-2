#pragma once
#include <QMainWindow>
#include "image.h"
#include <vector>
#include <memory>

class QGridLayout;
class QScrollArea;
class QLabel;
class QProgressBar;

class GalleryWindow : public QMainWindow {
    Q_OBJECT
public:
    explicit GalleryWindow(QWidget* parent = nullptr);

private slots:
    void openFolder();
    void loadDirect();
    void loadWithProxy();

private:
    void clearGallery();
    void showThumbnails();
    void onThumbnailClicked(int index);
    void updateThumbnail(int index);
    void showPreview(int index);
    void updateStatusInfo();

    QStringList files_;
    std::vector<std::unique_ptr<Image>> images_;

    // Left panel - thumbnails
    QScrollArea* scrollArea_;
    QWidget* gridWidget_;
    QGridLayout* gridLayout_;

    // Right panel - preview
    QLabel* previewLabel_;
    QLabel* previewInfo_;

    // Status
    QLabel* statusLabel_;
    QLabel* modeLabel_;
    QProgressBar* progressBar_;

    bool usingProxy_ = false;
    int selectedIndex_ = -1;

    static constexpr int THUMB_SIZE = 140;
    static constexpr int COLUMNS = 3;
};