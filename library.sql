SELECT tr.title, tr.duration, ar.name, al.name FROM track tr
INNER JOIN ALBUM al ON al.id = tr.album_id
INNER JOIN ARTIST ar ON ar.id = al.artist_id
WHERE ar.name != ''
ORDER BY ar.name IS NULL, ar.name ASC, al.year ASC, al.name ASC, tr.track_number ASC;