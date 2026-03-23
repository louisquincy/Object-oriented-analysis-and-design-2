#pragma once
#include <QPixmap>
#include <QString>

class Image {
public:
    virtual ~Image() = default;
    virtual QPixmap getPixmap(int width, int height) = 0;
    virtual QString getFilename() const = 0;
    virtual bool isLoaded() const = 0;
};