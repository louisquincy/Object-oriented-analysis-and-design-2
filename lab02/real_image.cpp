#include "real_image.h"
#include <QFileInfo>
#include <QDebug>
#include <QThread>

RealImage::RealImage(const QString& filepath) : filepath_(filepath) {
    loadFromDisk();
}

void RealImage::loadFromDisk() {
    qDebug() << "[RealImage] Loading:" << QFileInfo(filepath_).fileName();

    QThread::msleep(200);

    pixmap_.load(filepath_);
    loaded_ = !pixmap_.isNull();
}

QPixmap RealImage::getPixmap(int width, int height) {
    if (!loaded_) return {};
    return pixmap_.scaled(width, height, Qt::KeepAspectRatio, Qt::SmoothTransformation);
}

QString RealImage::getFilename() const {
    return QFileInfo(filepath_).fileName();
}

bool RealImage::isLoaded() const {
    return loaded_;
}
