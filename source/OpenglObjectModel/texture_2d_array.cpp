#include "texture_2d_array.hpp"

#include <utility>
#include <iostream>

#include "io/file_api.hpp"


namespace opengl
{
  Texture2DArray::Texture2DArray(const std::vector<blocks::Image>& images, const int resolutionX, const int resolutionY)
  {
    glGenTextures(1, &id_);
    glBindTexture(GL_TEXTURE_2D_ARRAY, id_);
    glTexImage3D(GL_TEXTURE_2D_ARRAY, 0, GL_RGBA, resolutionX, resolutionY, 6, 0, GL_RGBA, GL_UNSIGNED_BYTE, nullptr);

    for (int i = 0; i < images.size(); i++)
    {
      const blocks::Image& image = images[i];
      if (!image.data.empty())
      {
        GLuint channelsMode = GL_RGB;
        if (image.channels == 4)
          channelsMode = GL_RGBA;

        glTexSubImage3D(GL_TEXTURE_2D_ARRAY, 0, 0, 0, i, resolutionX, resolutionY, 1, channelsMode, GL_UNSIGNED_BYTE, &image.data[0]);
        glGenerateMipmap(GL_TEXTURE_2D);
      }
      else
      {
        std::cout << "Failed to load texture" << std::endl;
      }
    }

    glTexParameterf(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
    glTexParameterf(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
  }

  Texture2DArray::Texture2DArray(Texture2DArray&& other) : Object(other.id_)
  {
    other.id_ = 0;
  }

  Texture2DArray& Texture2DArray::operator=(Texture2DArray&& other)
  {
    if (this == &other)
    {
      return *this;
    }

    Release();
    std::swap(id_, other.id_);

    return *this;
  }

  Texture2DArray::~Texture2DArray()
  {
    Release();
  }


  void Texture2DArray::Bind(int slot)
  {
    glActiveTexture(GL_TEXTURE0 + slot);
    glBindTexture(GL_TEXTURE_2D_ARRAY, id_);
  }


  void Texture2DArray::Release()
  {
    if (id_ != 0)
    {
      glDeleteTextures(1, &id_);
      id_ = 0;
    }
  }
}
