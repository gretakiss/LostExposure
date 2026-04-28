using System.Collections.Generic;
using UnityEngine;

// Ez az osztály tárolja a küldetés során készült fotókat.
// Static, így scene váltás után is megmarad.
public static class MissionPhotoArchive
{
    private static List<Texture2D> photos = new List<Texture2D>();

    // Hány képen volt rajta az entity
    private static int entityPhotoCount = 0;

    public static void Clear()
    {
        photos.Clear();
        entityPhotoCount = 0;
    }

    // Fotó hozzáadása + jelöljük, hogy entity rajta volt-e
    public static void AddPhoto(Texture2D photo, bool containsEntity)
    {
        if (photo == null)
            return;

        photos.Add(photo);

        if (containsEntity)
            entityPhotoCount++;
    }

    public static int Count()
    {
        return photos.Count;
    }

    public static int EntityPhotoCount()
    {
        return entityPhotoCount;
    }

    public static Texture2D GetPhoto(int index)
    {
        if (photos.Count == 0) return null;
        if (index < 0 || index >= photos.Count) return null;

        return photos[index];
    }
}