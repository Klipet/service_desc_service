using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

public class S3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3Service(IConfiguration config)
    {
        var accessKey = config["AwsS3:AccessKey"];
        var secretKey = config["AwsS3:SecretKey"];
        var region = RegionEndpoint.GetBySystemName(config["AwsS3:Region"]);

        _bucketName = config["AwsS3:BucketName"];
        _s3Client = new AmazonS3Client(accessKey, secretKey, region);
    }

    public async Task<string> UploadFileAsync(IFormFile file, string key)
    {
        using var stream = file.OpenReadStream();

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = stream,
            ContentType = file.ContentType
        };

        await _s3Client.PutObjectAsync(request);

        return $"https://{_bucketName}.s3.amazonaws.com/{key}";
    }

    public string GeneratePreSignedUrl(string key)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(10)
        };

        return _s3Client.GetPreSignedURL(request);
    }
}
