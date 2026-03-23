#pragma once
#include "image.h"

class RealImage : public Image {
public:
    explicit RealImage(const QString& filepath);

    QPixmap getPixmap(int width, int height) override;
    QString getFilename() const override;
    bool isLoaded() const override;

private:
    QString filepath_;
    QPixmap pixmap_;
    bool loaded_ = false;

    void loadFromDisk();
};