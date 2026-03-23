#include "image_proxy.h"
#include <QFileInfo>
#include <QDebug>

ImageProxy::ImageProxy(const QString& filepath)
    : filepath_(filepath), realImage_(nullptr)
{
    qDebug() << "[Proxy] Created for:" << QFileInfo(filepath_).fileName();
}

QPixmap ImageProxy::getPixmap(int width, int height) {
    if (!realImage_) {
        qDebug() << "[Proxy] First access -> creating RealImage...";
        realImage_ = std::make_unique<RealImage>(filepath_);
    }
    return realImage_->getPixmap(width, height);
}

QString ImageProxy::getFilename() const {
    return QFileInfo(filepath_).fileName();
}

bool ImageProxy::isLoaded() const {
    return realImage_ && realImage_->isLoaded();
}
