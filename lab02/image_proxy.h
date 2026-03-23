#pragma once
#include "image.h"
#include "real_image.h"
#include <memory>

class ImageProxy : public Image {
public:
    explicit ImageProxy(const QString& filepath);

    QPixmap getPixmap(int width, int height) override;
    QString getFilename() const override;
    bool isLoaded() const override;

private:
    QString filepath_;
    std::unique_ptr<RealImage> realImage_;  // nullptr до первого обращения
};