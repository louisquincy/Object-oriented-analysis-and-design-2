#include "gallery_window.h"
#include "real_image.h"
#include "image_proxy.h"

#include <QToolBar>
#include <QFileDialog>
#include <QDir>
#include <QScrollArea>
#include <QGridLayout>
#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QLabel>
#include <QStatusBar>
#include <QSplitter>
#include <QElapsedTimer>
#include <QMouseEvent>
#include <QApplication>
#include <QProgressBar>
#include <QFrame>
#include <QFont>

// --- Clickable thumbnail widget ---
class ThumbnailWidget : public QFrame {
public:
    ThumbnailWidget(int index, std::function<void(int)> onClick, QWidget* parent = nullptr)
        : QFrame(parent), index_(index), onClick_(onClick)
    {
        setCursor(Qt::PointingHandCursor);
        setFixedSize(150, 180);
        setFrameStyle(QFrame::Box);
        setStyleSheet(
            "ThumbnailWidget { background: #f5f5f5; border: 2px solid #ddd; border-radius: 6px; }"
            "ThumbnailWidget:hover { border-color: #4a90d9; background: #eef4fb; }"
        );

        auto* layout = new QVBoxLayout(this);
        layout->setContentsMargins(4, 4, 4, 4);
        layout->setSpacing(2);

        imageLabel_ = new QLabel;
        imageLabel_->setFixedSize(140, 140);
        imageLabel_->setAlignment(Qt::AlignCenter);
        imageLabel_->setStyleSheet("border: none; background: transparent;");

        nameLabel_ = new QLabel;
        nameLabel_->setAlignment(Qt::AlignCenter);
        nameLabel_->setStyleSheet("border: none; font-size: 10px; color: #666;");
        nameLabel_->setMaximumWidth(140);

        layout->addWidget(imageLabel_);
        layout->addWidget(nameLabel_);
    }

    void setThumbnail(const QPixmap& pix) {
        imageLabel_->setPixmap(pix);
        imageLabel_->setStyleSheet("border: none; background: transparent;");
    }

    void setPlaceholder(const QString& text) {
        imageLabel_->setText(text);
        imageLabel_->setStyleSheet(
            "border: none; background: #e8e8e8; color: #999; "
            "font-size: 12px; border-radius: 4px;"
        );
    }

    void setLoading() {
        imageLabel_->setText("Loading...");
        imageLabel_->setStyleSheet(
            "border: none; background: #fff3cd; color: #856404; "
            "font-size: 12px; border-radius: 4px;"
        );
    }

    void setName(const QString& name) {
        nameLabel_->setText(name);
    }

    void setSelected(bool selected) {
        if (selected)
            setStyleSheet(
                "ThumbnailWidget { background: #eef4fb; border: 2px solid #2d6fbf; border-radius: 6px; }"
            );
        else
            setStyleSheet(
                "ThumbnailWidget { background: #f5f5f5; border: 2px solid #ddd; border-radius: 6px; }"
                "ThumbnailWidget:hover { border-color: #4a90d9; background: #eef4fb; }"
            );
    }

protected:
    void mousePressEvent(QMouseEvent*) override { if (onClick_) onClick_(index_); }

private:
    int index_;
    std::function<void(int)> onClick_;
    QLabel* imageLabel_;
    QLabel* nameLabel_;
};

// --- Main Window ---
GalleryWindow::GalleryWindow(QWidget* parent) : QMainWindow(parent) {
    setWindowTitle("Image Viewer — Proxy Pattern Demo");
    resize(1000, 650);
    setMinimumSize(800, 500);

    // Toolbar
    auto* toolbar = addToolBar("Main");
    toolbar->setMovable(false);
    toolbar->setIconSize(QSize(24, 24));
    toolbar->setToolButtonStyle(Qt::ToolButtonTextOnly);

    auto* openAction = toolbar->addAction("📂 Open Folder");
    connect(openAction, &QAction::triggered, this, &GalleryWindow::openFolder);
    toolbar->addSeparator();

    auto* directAction = toolbar->addAction("⚡ Direct Load");
    directAction->setToolTip("Load ALL images immediately (no proxy)");
    connect(directAction, &QAction::triggered, this, &GalleryWindow::loadDirect);

    auto* proxyAction = toolbar->addAction("🔄 Smart Load (Proxy)");
    proxyAction->setToolTip("Create lightweight proxies, load on demand");
    connect(proxyAction, &QAction::triggered, this, &GalleryWindow::loadWithProxy);

    toolbar->addSeparator();
    modeLabel_ = new QLabel(" No images loaded ");
    modeLabel_->setStyleSheet("font-weight: bold; padding: 0 10px;");
    toolbar->addWidget(modeLabel_);

    // Main layout: thumbnails | preview
    auto* splitter = new QSplitter(Qt::Horizontal);

    // Left: scrollable grid of thumbnails
    scrollArea_ = new QScrollArea;
    gridWidget_ = new QWidget;
    gridLayout_ = new QGridLayout(gridWidget_);
    gridLayout_->setSpacing(8);
    gridLayout_->setContentsMargins(8, 8, 8, 8);
    gridLayout_->setAlignment(Qt::AlignTop | Qt::AlignLeft);
    scrollArea_->setWidget(gridWidget_);
    scrollArea_->setWidgetResizable(true);
    scrollArea_->setMinimumWidth(350);

    // Right: preview panel
    auto* previewPanel = new QWidget;
    auto* previewLayout = new QVBoxLayout(previewPanel);
    previewLayout->setContentsMargins(10, 10, 10, 10);

    previewLabel_ = new QLabel("Select an image\nto preview");
    previewLabel_->setAlignment(Qt::AlignCenter);
    previewLabel_->setMinimumSize(400, 400);
    previewLabel_->setStyleSheet(
        "background: #fafafa; border: 2px dashed #ccc; "
        "border-radius: 8px; color: #aaa; font-size: 16px;"
    );

    previewInfo_ = new QLabel("");
    previewInfo_->setAlignment(Qt::AlignCenter);
    previewInfo_->setStyleSheet("color: #555; font-size: 12px; padding: 5px;");
    previewInfo_->setWordWrap(true);

    previewLayout->addWidget(previewLabel_, 1);
    previewLayout->addWidget(previewInfo_);

    splitter->addWidget(scrollArea_);
    splitter->addWidget(previewPanel);
    splitter->setStretchFactor(0, 2);
    splitter->setStretchFactor(1, 3);

    setCentralWidget(splitter);

    // Status bar
    progressBar_ = new QProgressBar;
    progressBar_->setFixedWidth(150);
    progressBar_->setVisible(false);

    statusLabel_ = new QLabel("Ready. Open a folder to start.");
    statusBar()->addWidget(statusLabel_, 1);
    statusBar()->addPermanentWidget(progressBar_);
}

void GalleryWindow::openFolder() {
    QString dir = QFileDialog::getExistingDirectory(this, "Select folder with images");
    if (dir.isEmpty()) return;

    QDir d(dir);
    QStringList filters = {"*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif", "*.webp"};
    QStringList names = d.entryList(filters, QDir::Files);

    files_.clear();
    for (const auto& name : names)
        files_ << d.absoluteFilePath(name);

    clearGallery();
    statusLabel_->setText(QString("Found %1 images. Choose loading mode.").arg(files_.size()));
    modeLabel_->setText(QString(" %1 images found ").arg(files_.size()));
    previewLabel_->setText("Select loading mode\nfrom toolbar");
    previewInfo_->setText(QString("Folder: %1").arg(dir));
}

void GalleryWindow::loadDirect() {
    if (files_.isEmpty()) {
        statusLabel_->setText("Open a folder first!");
        return;
    }

    clearGallery();
    usingProxy_ = false;

    modeLabel_->setText(" ⚡ DIRECT MODE ");
    modeLabel_->setStyleSheet("font-weight: bold; padding: 0 10px; color: #c0392b;");

    progressBar_->setVisible(true);
    progressBar_->setRange(0, files_.size());
    progressBar_->setValue(0);

    QElapsedTimer timer;
    timer.start();

    for (int i = 0; i < files_.size(); ++i) {
        statusLabel_->setText(QString("Loading %1 / %2 ...").arg(i + 1).arg(files_.size()));
        images_.push_back(std::make_unique<RealImage>(files_[i]));
        progressBar_->setValue(i + 1);
        QApplication::processEvents();
    }

    qint64 elapsed = timer.elapsed();

    showThumbnails();
    progressBar_->setVisible(false);

    statusLabel_->setText(
        QString("Direct: ALL %1 images loaded in %2 ms. Memory: all in RAM.")
        .arg(images_.size()).arg(elapsed)
    );

    previewLabel_->setText(QString("All %1 images loaded\nin %2 ms\n\nClick any thumbnail to preview")
        .arg(images_.size()).arg(elapsed));
    previewInfo_->setText("Mode: Direct Load — all images were loaded immediately into memory.");
}

void GalleryWindow::loadWithProxy() {
    if (files_.isEmpty()) {
        statusLabel_->setText("Open a folder first!");
        return;
    }

    clearGallery();
    usingProxy_ = true;

    QElapsedTimer timer;
    timer.start();

    for (const auto& file : files_) {
        images_.push_back(std::make_unique<ImageProxy>(file));
    }

    qint64 elapsed = timer.elapsed();

    modeLabel_->setText(" 🔄 PROXY MODE ");
    modeLabel_->setStyleSheet("font-weight: bold; padding: 0 10px; color: #27ae60;");

    showThumbnails();

    statusLabel_->setText(
        QString("Proxy: %1 proxies created in %2 ms. NO images in memory yet. Click to load.")
        .arg(images_.size()).arg(elapsed)
    );

    previewLabel_->setText(
        QString("%1 proxies created\nin %2 ms\n\n"
                "No images loaded yet!\n"
                "Click a thumbnail to load it on demand")
        .arg(images_.size()).arg(elapsed));
    previewInfo_->setText("Mode: Proxy (Lazy Loading) — images load only when you click them.");
}

void GalleryWindow::showThumbnails() {
    for (int i = 0; i < static_cast<int>(images_.size()); ++i) {
        auto* thumb = new ThumbnailWidget(i,
            [this](int idx) { onThumbnailClicked(idx); }, gridWidget_);

        thumb->setName(images_[i]->getFilename());

        if (images_[i]->isLoaded()) {
            thumb->setThumbnail(images_[i]->getPixmap(THUMB_SIZE, THUMB_SIZE));
        } else {
            thumb->setPlaceholder("Click\nto load");
        }

        gridLayout_->addWidget(thumb, i / COLUMNS, i % COLUMNS);
    }
}

void GalleryWindow::onThumbnailClicked(int index) {
    if (index < 0 || index >= static_cast<int>(images_.size())) return;

    // Deselect previous
    if (selectedIndex_ >= 0) {
        auto* prev = gridLayout_->itemAtPosition(selectedIndex_ / COLUMNS, selectedIndex_ % COLUMNS);
        if (prev) {
            if (auto* w = dynamic_cast<ThumbnailWidget*>(prev->widget()))
                w->setSelected(false);
        }
    }

    selectedIndex_ = index;

    // Select current
    auto* item = gridLayout_->itemAtPosition(index / COLUMNS, index % COLUMNS);
    if (!item) return;
    auto* thumb = dynamic_cast<ThumbnailWidget*>(item->widget());
    if (!thumb) return;

    thumb->setSelected(true);

    bool wasLoaded = images_[index]->isLoaded();

    if (!wasLoaded) {
        thumb->setLoading();
        QApplication::processEvents();
    }

    QElapsedTimer timer;
    timer.start();
    QPixmap pix = images_[index]->getPixmap(THUMB_SIZE, THUMB_SIZE);
    qint64 elapsed = timer.elapsed();

    // Update thumbnail
    if (!pix.isNull()) {
        thumb->setThumbnail(pix);
    }

    // Show large preview
    showPreview(index);

    // Update status
    if (!wasLoaded && images_[index]->isLoaded()) {
        statusLabel_->setText(
            QString("'%1' loaded in %2 ms")
            .arg(images_[index]->getFilename()).arg(elapsed)
        );
    } else {
        statusLabel_->setText(
            QString("Viewing: %1").arg(images_[index]->getFilename())
        );
    }

    updateStatusInfo();
}

void GalleryWindow::showPreview(int index) {
    if (index < 0 || index >= static_cast<int>(images_.size())) return;

    // Get a larger version for preview
    QPixmap pix = images_[index]->getPixmap(
        previewLabel_->width() - 20,
        previewLabel_->height() - 20
    );

    if (!pix.isNull()) {
        previewLabel_->setPixmap(pix);
        previewLabel_->setStyleSheet(
            "background: #fafafa; border: 1px solid #ddd; border-radius: 8px;"
        );
    } else {
        previewLabel_->setText("Failed to load image");
    }

    previewInfo_->setText(
        QString("%1\nSize: %2 × %3")
        .arg(images_[index]->getFilename())
        .arg(pix.width()).arg(pix.height())
    );
}

void GalleryWindow::updateStatusInfo() {
    int loaded = 0;
    for (const auto& img : images_)
        if (img->isLoaded()) loaded++;

    QString mode = usingProxy_ ? "Proxy" : "Direct";
    modeLabel_->setText(
        QString(" %1: %2 / %3 loaded ")
        .arg(mode).arg(loaded).arg(images_.size())
    );
}

void GalleryWindow::clearGallery() {
    images_.clear();
    selectedIndex_ = -1;
    while (auto* item = gridLayout_->takeAt(0)) {
        delete item->widget();
        delete item;
    }
    previewLabel_->clear();
    previewLabel_->setText("Select an image\nto preview");
    previewLabel_->setStyleSheet(
        "background: #fafafa; border: 2px dashed #ccc; "
        "border-radius: 8px; color: #aaa; font-size: 16px;"
    );
    previewInfo_->clear();
}